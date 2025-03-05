from collections import defaultdict
from datetime import datetime
from fastapi import WebSocket, WebSocketDisconnect, APIRouter
from typing import List, Dict, Tuple
from beanie import Document, PydanticObjectId
from pydantic import BaseModel


class PatrolUpdate(BaseModel):
    patrol_id: str
    position: list[float]
    orientation: float


class PatrolBase(BaseModel):
    model_file_id: str
    start_time: datetime
    path: List[PatrolUpdate]


class Patrol(Document, PatrolBase):
    pass


class PatrolOut(PatrolBase):
    id: PydanticObjectId


# ------------------------ 改动从这里开始 ------------------------ #
class ConnectionManager:
    def __init__(self):
        # key: model_file_id, value: list of WebSocket connections
        self.connections_per_model = defaultdict(list)

    async def connect(self, model_file_id: str, websocket: WebSocket):
        # 接受 WebSocket 连接
        await websocket.accept()
        # 将该连接加入对应 model_file_id 的列表
        self.connections_per_model[model_file_id].append(websocket)
        print(
            f"New Connection to model_file_id={model_file_id}. "
            f"Current Amount={len(self.connections_per_model[model_file_id])}"
        )

    def disconnect(self, model_file_id: str, websocket: WebSocket):
        # 从对应 model_file_id 的列表中移除
        if websocket in self.connections_per_model[model_file_id]:
            self.connections_per_model[model_file_id].remove(websocket)
            print(
                f"End Connection from model_file_id={model_file_id}. "
                f"Current Amount={len(self.connections_per_model[model_file_id])}"
            )

    async def broadcast(self, model_file_id: str, message: str, sender: WebSocket):
        # 仅在同一个 model_file_id 的连接里进行广播
        for connection in self.connections_per_model[model_file_id]:
            if connection != sender:
                await connection.send_text(message)


manager = ConnectionManager()
patrol_router = APIRouter(prefix="/api/vi/model_files/{model_file_id}/patrol", tags=["patrol"])
active_paths: Dict[Tuple[str, str], Dict] = {}


@patrol_router.websocket("/ws")
async def websocket_endpoint(websocket: WebSocket, model_file_id: str):
    await manager.connect(model_file_id, websocket)
    try:
        while True:
            data = await websocket.receive_text()
            try:
                update = PatrolUpdate.parse_raw(data)
                print(f"Received：{update}")
            except Exception as e:
                print("Fail to parse data:", e)
                continue

            # 按 model_file_id 广播，不同 model_file_id 的连接互不影响
            await manager.broadcast(model_file_id, data, sender=websocket)

            # 下面是你原本存储路径的逻辑，可根据业务需要进行处理
            now = datetime.now()
            key = (update.patrol_id, model_file_id)  # 也可以自定义 worker_id/patrol_id 的组合
            if key not in active_paths:
                active_paths[key] = {
                    "last_saved": datetime.min,
                    "worker_path": Patrol(
                        model_file_id=model_file_id,
                        start_time=now,
                        path=[]
                    )
                }

            last_saved = active_paths[key]["last_saved"]
            if (now - last_saved).total_seconds() > 1:
                worker_path: Patrol = active_paths[key]["worker_path"]
                worker_path.path.append(update)
                active_paths[key]["last_saved"] = now
                await worker_path.save()

    except WebSocketDisconnect:
        manager.disconnect(model_file_id, websocket)


@patrol_router.get("/get_all", response_model=list[PatrolOut])
async def get_history(model_file_id: str):
    result = await Patrol.find(Patrol.model_file_id == model_file_id).to_list()
    return [PatrolOut(**x.dict()) for x in result]
