from functools import lru_cache

from pydantic import Field
from pydantic_settings import BaseSettings, SettingsConfigDict


class Settings(BaseSettings):
    backend_base_url: str = "http://localhost:5000"
    internal_service_key: str = ""
    service_timeout_seconds: float = Field(default=5.0, ge=0.1, le=60.0)
    tool_retry_count: int = Field(default=1, ge=0, le=3)
    max_input_chars: int = Field(default=16_000, ge=1_000, le=100_000)
    max_output_chars: int = Field(default=24_000, ge=2_000, le=100_000)
    max_graph_steps: int = Field(default=4, ge=1, le=4)

    model_config = SettingsConfigDict(
        env_file=".env",
        env_file_encoding="utf-8",
        case_sensitive=False,
        extra="ignore",
    )


@lru_cache
def get_settings() -> Settings:
    return Settings()
