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
class IssueIn(BaseModel):
    issue_type: str  # Issue type, such as "quality", "safety", "other"
    description: Optional[str] = ""  # Issue description
    image_url: Optional[str] = None  # Optional image URL
    camera_pose: Optional[CameraPose] = None  # Record the current camera pose
    position_3d: List[float]  # The 3D coordinate of the issue


# Base model for an issue, extends the creation model and adds time information
class IssueBase(IssueIn):
    create_time: datetime
    update_time: Optional[datetime] = None
    model_file_id: str


# Issue document model for storing in the database
class Issue(Document, IssueBase):
    pass


# Output model for returning issues, including the database-generated id field
class IssueOut(IssueBase):
    id: PydanticObjectId


# Create an API router for issues
issue_router = APIRouter(tags=["issues"], prefix="/api/v1/model_files/{model_file_id}/issues")


@issue_router.get("/", response_model=List[IssueOut])
async def get_all_issues(model_file_id: str):
    issues = await Issue.find(Issue.model_file_id == model_file_id).to_list()
    return [IssueOut(**issue.dict()) for issue in issues]


@issue_router.get("/{id}", response_model=IssueOut)
async def get_issue(id: str, model_file_id: str):
    issue = await Issue.get(ObjectId(id))
    if issue or issue.model_file_id != model_file_id:
        return IssueOut(**issue.dict())
    else:
        raise HTTPException(status_code=404, detail="Issue not found")


@issue_router.post("/", response_model=IssueOut)
async def create_issue(data: IssueIn, model_file_id: str):
    issue = Issue(**data.dict() | {"model_file_id": model_file_id}, create_time=datetime.now())
    await issue.insert()
    return IssueOut(**issue.dict())


@issue_router.put("/{id}", response_model=IssueOut)
async def update_issue(id: str, data: IssueIn, model_file_id: str):
    issue = await Issue.get(ObjectId(id))
    if not issue or issue.model_file_id != model_file_id:
        raise HTTPException(status_code=404, detail="Issue not found")
    update_data = data.dict(exclude_unset=True)
    update_data['update_time'] = datetime.now()
    await issue.update({"$set": update_data})
    updated_issue = await Issue.get(ObjectId(id))
    return IssueOut(**updated_issue.dict())


@issue_router.delete("/{id}")
async def delete_issue(id: str, model_file_id: str):
    issue = await Issue.get(ObjectId(id))
    if not issue or issue.model_file_id != model_file_id:
        raise HTTPException(status_code=404, detail="Issue not found")
    await issue.delete()
    return {"detail": "Issue deleted"}
