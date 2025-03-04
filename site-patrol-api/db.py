from typing import Type

from beanie import init_beanie, Document
from fastapi import FastAPI
from motor.motor_asyncio import AsyncIOMotorClient
from pydantic_settings import BaseSettings


class DbSettings(BaseSettings):
    mongodb_url: str = ""
    mongodb_db: str = ""
    redis_url: str = ""


settings = DbSettings()
db_client = AsyncIOMotorClient(settings.mongodb_url)


def init_crud(app: FastAPI, db_models: list[Type["Document"]]):
    @app.on_event("startup")
    async def on_startup():
        global db_client
        await init_beanie(db_client[settings.mongodb_db], document_models=db_models)

    @app.on_event("shutdown")
    def on_shutdown():
        global db_client
        db_client.close()
