from datetime import datetime
from typing import List, Optional
from fastapi import APIRouter, HTTPException
from bson import ObjectId
from beanie import Document, PydanticObjectId
from pydantic import BaseModel


# Define the camera pose model to record device pose
class CameraPose(BaseModel):
    position: List[float]  # e.g., [x, y, z]
    orientation: List[float]  # e.g., [pitch, yaw, roll]


# Input model for creating an issue
class IssueCreateIn(BaseModel):
    issue_type: str  # Issue type, such as "quality", "safety", "other"
    description: Optional[str] = ""  # Issue description
    image_url: Optional[str] = None  # Optional image URL
    camera_pose: Optional[CameraPose] = None  # Record the current camera pose
    position_3d: List[float]  # The 3D coordinate of the issue


# Input model for updating an issue; all fields are optional
class IssueUpdateIn(BaseModel):
    issue_type: Optional[str] = None
    description: Optional[str] = None
    image_url: Optional[str] = None
    camera_pose: Optional[CameraPose] = None
    position_3d: Optional[List[float]] = None


# Base model for an issue, extends the creation model and adds time information
class IssueBase(IssueCreateIn):
    create_time: datetime
    update_time: Optional[datetime] = None


# Issue document model for storing in the database
class Issue(Document, IssueBase):
    pass


# Output model for returning issues, including the database-generated id field
class IssueOut(IssueBase):
    id: PydanticObjectId


# Create an API router for issues
issue_router = APIRouter(tags=["issues"], prefix="/api/v1/issues")


@issue_router.get("/", response_model=List[IssueOut])
async def get_all_issues():
    issues = await Issue.find_all().to_list()
    return [IssueOut(**issue.dict()) for issue in issues]


@issue_router.get("/{id}", response_model=IssueOut)
async def get_issue(id: str):
    issue = await Issue.get(ObjectId(id))
    if issue:
        return IssueOut(**issue.dict())
    else:
        raise HTTPException(status_code=404, detail="Issue not found")


@issue_router.post("/", response_model=IssueOut)
async def create_issue(data: IssueCreateIn):
    issue = Issue(**data.dict(), create_time=datetime.now())
    await issue.insert()
    return IssueOut(**issue.dict())


@issue_router.put("/{id}", response_model=IssueOut)
async def update_issue(id: str, data: IssueUpdateIn):
    issue = await Issue.get(ObjectId(id))
    if not issue:
        raise HTTPException(status_code=404, detail="Issue not found")
    update_data = data.dict(exclude_unset=True)
    update_data['update_time'] = datetime.now()
    await issue.update({"$set": update_data})
    updated_issue = await Issue.get(ObjectId(id))
    return IssueOut(**updated_issue.dict())


@issue_router.delete("/{id}")
async def delete_issue(id: str):
    issue = await Issue.get(ObjectId(id))
    if not issue:
        raise HTTPException(status_code=404, detail="Issue not found")
    await issue.delete()
    return {"detail": "Issue deleted"}
