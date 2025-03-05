from fastapi import APIRouter, UploadFile
from fastapi.responses import RedirectResponse
import aioboto3
import uuid
from pydantic_settings import BaseSettings
from pydantic import BaseModel
from urllib import parse

PATH_PREFIX = "uploads/site_patrol/"


class FileSettings(BaseSettings):
    s3_bucket: str = ""
    s3_region: str = ""
    s3_access_key: str = ""
    s3_secret_key: str = ""


class PreSignedUrl(BaseModel):
    upload_url: str
    download_url: str


class FileInfo(BaseModel):
    file_name: str


settings = FileSettings()
file_router = APIRouter(tags=["file"], prefix="/api/v1/file")


@file_router.post("/upload")
async def upload(file: UploadFile):
    # 这里使用 with 没问题，上传完就结束了
    async with make_s3_client() as client:
        file_path = f"{uuid.uuid4()}/{file.filename}"
        await client.upload_fileobj(file.file, settings.s3_bucket, PATH_PREFIX + file_path)
        return f"/api/v1/file/download?file_path={parse.quote(file_path)}"


@file_router.get("/download")
async def download(file_path: str):
    async with make_s3_client() as client:
        download_url = await client.generate_presigned_url(
            "get_object",
            Params={"Bucket": settings.s3_bucket, "Key": PATH_PREFIX + file_path},
            ExpiresIn=3600,
        )
    return RedirectResponse(download_url)


def make_s3_client():
    return aioboto3.Session().client(
        's3',
        aws_access_key_id=settings.s3_access_key,
        aws_secret_access_key=settings.s3_secret_key,
        region_name=settings.s3_region
    )
