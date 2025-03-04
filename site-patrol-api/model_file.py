from datetime import datetime
from beanie import Document
from fastapi import APIRouter, HTTPException
from bson import ObjectId
from beanie import PydanticObjectId
from pydantic import BaseModel


class Marker(BaseModel):
    id: str
    position: list[float]
    orientation: list[float]


class UpdateMarkersIn(BaseModel):
    markers: list[Marker] = []


class ModelFileCreateIn(BaseModel):
    file_url: str


class ModelFileBase(UpdateMarkersIn, ModelFileCreateIn):
    create_time: datetime
    pass


class ModelFile(Document, ModelFileBase):
    pass


class ModelFileOut(ModelFileBase):
    id: PydanticObjectId


model_file_router = APIRouter(tags=["model_files"], prefix="/api/v1/model_files")


@model_file_router.get("/", response_model=list[ModelFileOut])
async def get_all():
    db_data = await ModelFile.find_all().to_list()
    return [ModelFileOut(**doc.dict()) for doc in db_data]


@model_file_router.get("/{id}", response_model=ModelFileOut)
async def get_model_file(id: str):
    model_file = await ModelFile.get(ObjectId(id))
    if model_file:
        return ModelFileOut(**model_file.dict())
    else:
        raise HTTPException(status_code=404, detail="not existed")


@model_file_router.post("/", response_model=ModelFileOut)
async def create_model_file(data: ModelFileCreateIn):
    model_file = ModelFile(**data.dict(), create_time=datetime.now())
    await model_file.insert()
    return ModelFileOut(**model_file.dict())


@model_file_router.put("/{id}/markers")
async def update_markers(id: str, data: UpdateMarkersIn):
    model_file = await ModelFile.get(ObjectId(id))
    if not model_file:
        raise HTTPException(status_code=404, detail="not existed")
    await model_file.update({"$set": data.dict()})


@model_file_router.delete("/{id}")
async def delete_model_file(id: str):
    model_file = await ModelFile.get(ObjectId(id))
    if not model_file:
        raise HTTPException(status_code=404, detail="not existed")
    await model_file.delete()
