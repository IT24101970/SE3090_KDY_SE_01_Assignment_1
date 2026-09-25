from fastapi import FastAPI, Request
from fastapi.middleware.cors import CORSMiddleware
from fastapi.responses import JSONResponse

from app.api.routes import router
from app.config import get_settings


def create_app() -> FastAPI:
    application = FastAPI(
        title="ChannelCenter AI Service - Student 2 Agentic Subsystem",
        version="1.0.0",
    )

    # Configure CORS Middleware for cross-origin frontend requests
    application.add_middleware(
        CORSMiddleware,
        allow_origins=["*"],
        allow_credentials=True,
        allow_methods=["*"],
        allow_headers=["*"],
    )

    settings = get_settings()

    @application.middleware("http")
    async def enforce_body_limit(request: Request, call_next):
        if request.method == "OPTIONS":
            return await call_next(request)
        content_length = request.headers.get("content-length")
        if content_length and int(content_length) > settings.max_input_chars:
            return JSONResponse(status_code=413, content={"detail": "request body is too large"})
        return await call_next(request)

    application.include_router(router)
    return application


app = create_app()
