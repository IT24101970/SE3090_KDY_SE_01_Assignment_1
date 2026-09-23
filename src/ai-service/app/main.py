from fastapi import FastAPI, Request
from fastapi.responses import JSONResponse

from app.api.routes import router
from app.config import get_settings


def create_app() -> FastAPI:
    application = FastAPI(
        title="ChannelCenter Safety Auditor",
        version="1.0.0",
        docs_url=None,
        redoc_url=None,
    )
    settings = get_settings()

    @application.middleware("http")
    async def enforce_body_limit(request: Request, call_next):
        content_length = request.headers.get("content-length")
        if content_length and int(content_length) > settings.max_input_chars:
            return JSONResponse(status_code=413, content={"detail": "request body is too large"})
        return await call_next(request)

    application.include_router(router)
    return application


app = create_app()
