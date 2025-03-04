from datetime import datetime
from fastapi import WebSocket, WebSocketDisconnect, APIRouter
from typing import List, Dict, Tuple
from beanie import Document, PydanticObjectId
from pydantic import BaseModel


class Position(BaseModel):
    x: float
    y: float
    z: float


class PatrolUpdate(BaseModel):
    patrol_id: str
    position: Position
    orientation: float


class PatrolBase(BaseModel):
    model_file_id: str
    start_time: datetime
    path: List[PatrolUpdate]


class Patrol(Document, PatrolBase):
    pass


class PatrolOut(PatrolBase):
    id: PydanticObjectId


class ConnectionManager:
    active_connections: List[WebSocket] = []

    async def connect(self, websocket: WebSocket):
        await websocket.accept()
        self.active_connections.append(websocket)
        print(f"New Connection Current Amount：{len(self.active_connections)}")

    def disconnect(self, websocket: WebSocket):
        self.active_connections.remove(websocket)
        print(f"End Connection Current Amount：{len(self.active_connections)}")

    async def broadcast(self, message: str, sender: WebSocket):
        for connection in self.active_connections:
            if connection != sender:
                await connection.send_text(message)


manager = ConnectionManager()
patrol_router = APIRouter(prefix="/api/vi/patrol", tags=["patrol"])
active_paths: Dict[Tuple[str, str], Dict] = {}


@patrol_router.websocket("/ws")
async def websocket_endpoint(websocket: WebSocket):
    await manager.connect(websocket)
    try:
        while True:
            data = await websocket.receive_text()
            update = None
            try:
                update = PatrolUpdate.parse_raw(data)
                print(f"received：{update}")
            except Exception as e:
                print("fail to parse data:", e)
                continue

            await manager.broadcast(data, sender=websocket)

        now = datetime.now()
        key = (update.worker_id, update.patrol_id)
        if key not in active_paths:
            active_paths[key] = {"last_saved": datetime.min, "worker_path": Patrol(
                worker_id=update.worker_id,
                start_time=now,
                route=[]
            )}

        last_saved = active_paths[key]["last_saved"]
        if (now - last_saved).total_seconds() > 1:
            worker_path: Patrol = active_paths[key]["worker_path"]
            worker_path.path.append(update)
            active_paths[key]["last_saved"] = now
            await worker_path.save()

    except WebSocketDisconnect:
        manager.disconnect(websocket)


@patrol_router.get("/get_all", response_model=list[PatrolOut])
async def get_history(model_file_id: str = None):
    result = await Patrol.find(Patrol.model_file_id == model_file_id).to_list()
    return [PatrolOut(**x.dict()) for x in result]
