import uvicorn
from fastapi import FastAPI
from pydantic_settings import BaseSettings
from model_file import model_file_router, ModelFile
from file import file_router
from patrol import patrol_router, Patrol
from issue import issue_router, Issue
from db import init_crud


class ServerSettings(BaseSettings):
    host: str = "0.0.0.0"
    port: int = 8001
    debug_mode: bool = True


settings = ServerSettings()
app = FastAPI()
app.openapi_version = "3.0.2"

init_crud(app, [ModelFile, Patrol, Issue])

app.include_router(file_router)
app.include_router(model_file_router)
app.include_router(patrol_router)
app.include_router(issue_router)

if __name__ == "__main__":
    uvicorn.run(
        "main:app",
        host=settings.host,
        reload=settings.debug_mode,
        port=settings.port,
    )
