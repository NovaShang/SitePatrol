from fastapi import APIRouter, UploadFile
from fastapi.responses import StreamingResponse
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
    # 1) 不要在这里用 async with，而是先手动拿到异步 client
    client_context = make_s3_client()
    # 通过手动调用 __aenter__(), __aexit__() 来管理生命周期
    s3_client = await client_context.__aenter__()  # 打开异步 client

    try:
        # 2) 拿到 S3 对象
        response = await s3_client.get_object(Bucket=settings.s3_bucket, Key=PATH_PREFIX + file_path)
        content_type = response.get("ContentType", "application/octet-stream")
        filename = file_path.split("/")[-1]
        body = response["Body"]

        # 3) 定义流式生成器
        async def file_stream():
            try:
                async for chunk in body.iter_chunks():
                    yield chunk
            finally:
                # 当流式传输完毕（或异常中断）后，才会走到这里来关闭连接
                body.close()
                await client_context.__aexit__(None, None, None)

        # 4) 返回流响应
        return StreamingResponse(
            file_stream(),
            media_type=content_type,
            headers={"Content-Disposition": f'attachment; filename="{filename}"'}
        )
    except:
        # 如果在获取数据、或准备流式传输前就出错，也要记得关闭
        await client_context.__aexit__(None, None, None)
        raise


def make_s3_client():
    return aioboto3.Session().client(
        's3',
        aws_access_key_id=settings.s3_access_key,
        aws_secret_access_key=settings.s3_secret_key,
        region_name=settings.s3_region
    )
