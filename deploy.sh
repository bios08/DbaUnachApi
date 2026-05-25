#!/bin/bash

# ============================================
#  DbaUnachApi — Script de despliegue
#  Uso: ./deploy.sh
# ============================================

set -e  # Si cualquier comando falla, detiene el script

CONTAINER_NAME="UnachDbaApi"
IMAGE_NAME="imageUnachDbaApi"
PORT="8081"

echo "============================================"
echo " DbaUnachApi — Iniciando despliegue..."
echo "============================================"

# 1) Actualizar código desde Git
echo ""
echo "📥 Paso 1: Obteniendo últimos cambios..."
git pull origin master

# 2) Detener y eliminar el contenedor anterior (si existe)
echo ""
echo "🛑 Paso 2: Deteniendo contenedor anterior..."
if [ "$(docker ps -q -f name=$CONTAINER_NAME)" ]; then
    docker stop $CONTAINER_NAME
    echo "   Contenedor detenido."
fi

if [ "$(docker ps -aq -f name=$CONTAINER_NAME)" ]; then
    docker rm $CONTAINER_NAME
    echo "   Contenedor eliminado."
fi

# 3) Limpiar imagen anterior para liberar espacio
echo ""
echo "🧹 Paso 3: Limpiando imagen anterior..."
docker image prune -f

# 4) Construir nueva imagen
echo ""
echo "🔨 Paso 4: Construyendo imagen Docker..."
docker build -t $IMAGE_NAME .

# 5) Ejecutar nuevo contenedor
echo ""
echo "🚀 Paso 5: Iniciando nuevo contenedor..."
docker run -d -p $PORT:8080 --name $CONTAINER_NAME $IMAGE_NAME

# 6) Verificar que quedó corriendo
echo ""
echo "✅ Verificando estado..."
docker ps -f name=$CONTAINER_NAME

echo ""
echo "============================================"
echo " ¡Despliegue completado!"
echo " API disponible en http://localhost:$PORT"
echo "============================================"
