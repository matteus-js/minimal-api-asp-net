## REQUISITOS PARA RODAR O PROJETO
- .NET 8.0
- Docker

## RODANDO O PROJETO
1. subindo a imagem do MySql com docker
```bash
docker run --name minimal-api -p 3306:3306 -e MYSQL_ROOT_PASSWORD=root -d mysql:8.4.3
```
2. rondado a api

```bash
dotnet run
```
- documentação: http://localhost:5135/swagger

## RODANDO OS TESTES
```bash
dotnet tests
```