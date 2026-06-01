Elasticsearch и Kibana были развернуты с использованием `docker-compose`. Контейнеры подключены к общей Docker-сети (например, `nifi_net`) вместе с контейнером Apache NiFi:
<img width="668" height="867" alt="изображение" src="https://github.com/user-attachments/assets/8684cc68-e8a7-4ca3-a2e4-ee15cd0641c4" />
Создан и подготовлен каталог в файловой системе (или примонтированный том Docker), содержащий Parquet-файлы, сформированные в рамках практической работы №5:
<img width="949" height="368" alt="изображение" src="https://github.com/user-attachments/assets/604f1a51-7627-4b95-989b-1d15d6b45812" />
Настроил все процессы в Apache NiFi. Listfile настроен для мониторинга указанного каталога и обнаружения новых или измененных файлов с расширением `.parquet`. 
Fetchfile получает координаты файла от `ListFile` и считывает его содержимое, передавая его дальше по flow-файлу в виде attribute и content. Для корректного преобразования данных создана схема
(например, Avro Schema или через Schema Registry). Поле идентификатора документа (например, `id`) явно определено как строковый тип (`string`), что требуется для корректного маппинга в `_id` документа Elasticsearch.
ConvertRecorder преобразует формат данных из Parquet в JSON. PutElasticsear отвечает за загрузку JSON-документов в индекс Elasticsearch. Для предотвращения дублирования настроено использование поля идентификатора в качестве `_id` документа.
Pipeline был запущен. Процессоры успешно обработали файлы: `ListFile` обнаружил файлы, `FetchFile` их прочитал, `ConvertRecord` преобразовал в JSON, а `PutElasticsearchRecord` загрузил их в Elasticsearch.
<img width="1123" height="702" alt="изображение" src="https://github.com/user-attachments/assets/4f06115e-a9d7-4744-9dc0-807739e7e4b0" />
Проверка загрузки данных в Elasticsearch и корректного отображениях их:
<img width="1875" height="662" alt="изображение" src="https://github.com/user-attachments/assets/e6df03c5-5abd-466c-a711-18e8a30616cf" />
<img width="1570" height="733" alt="изображение" src="https://github.com/user-attachments/assets/ff185cba-3b7c-42b0-9494-8224f97f98f8" />
<img width="1416" height="702" alt="изображение" src="https://github.com/user-attachments/assets/5ecab7ed-1b82-4a5e-be0b-5bdaaa2bcb10" />
<img width="1435" height="515" alt="изображение" src="https://github.com/user-attachments/assets/63542f3e-24dd-4022-9130-c9e1380e55d5" />
В ходе практической работы был успешно реализован этап data pipeline для пакетной загрузки данных из формата Parquet в Elasticsearch.  
Основные достижения:
1. Настроен ETL-процесс в Apache NiFi с использованием связки процессоров `ListFile` → `FetchFile` → `ConvertRecord` → `PutElasticsearchRecord`.
2. Обеспечено корректное преобразование схемы данных, включая приведение поля идентификатора к строковому типу.
3. Реализована идемпотентность загрузки: повторная обработка файлов не приводит к созданию дубликатов документов в Elasticsearch благодаря использованию поля `_id`.
4. Все компоненты инфраструктурно изолированы и корректно взаимодействуют в рамках общей Docker-сети без модификации кода приложения ASP.NET Core.
