FROM python:3.12-slim as base

WORKDIR /app
COPY ./site-patrol-api/requirements.txt /app/requirements.txt
RUN pip install --no-cache-dir -r requirements.txt
COPY ./site-patrol-api /app

EXPOSE 80

CMD ["uvicorn", "main:app", "--host", "0.0.0.0", "--port", "80"]