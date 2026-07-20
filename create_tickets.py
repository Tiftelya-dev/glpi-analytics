import requests
import random
import time
from datetime import datetime, timedelta

# ==========================================
# НАСТРОЙКИ — измени под себя
# ==========================================

GLPI_URL = "http://localhost:8080/apirest.php"
APP_TOKEN = "UFM4A4v2FuTnup3X7My6erQoAdyspupiG7f8VWZl"
USER_TOKEN = "sRNI6Pma1s6zTryTGuiXliDW3x89cDreMfM0yeWU"

# Сколько заявок создать
TOTAL_TICKETS = 1000

# За сколько дней назад распределить заявки
DAYS_BACK = 30

# Задержка между запросами (0.05 = ~20 заявок в секунду)
DELAY = 0.01

# Распределение типов (сумма = 100)
TYPE_DISTRIBUTION = {
    1: 50,  # Инцидент — 50%
    2: 50,  # Запрос   — 50%
}

# Распределение приоритетов (сумма = 100)
PRIORITY_DISTRIBUTION = {
    6: 10,  # Критичный  — 10%
    4: 25,  # Высокий    — 25%
    3: 40,  # Умеренный  — 40%
    2: 25,  # Низкий     — 25%
}

# ==========================================
# ДАННЫЕ ДЛЯ ГЕНЕРАЦИИ
# ==========================================

INCIDENT_TITLES = [
    "Не запускается компьютер",
    "Синий экран смерти (BSOD)",
    "Не работает интернет",
    "Принтер не печатает",
    "Зависает операционная система",
    "Не открывается почта",
    "Вирус на компьютере",
    "Сломалась клавиатура",
    "Монитор не включается",
    "Не работает микрофон",
    "Ошибка при входе в систему",
    "Пропали файлы с рабочего стола",
    "Не работает веб-камера",
    "Компьютер перегревается",
    "Медленная работа компьютера",
    "Не подключается к Wi-Fi",
    "Ошибка 404 на сайте",
    "База данных недоступна",
    "Сервер не отвечает",
    "Потеря данных после обновления",
]

REQUEST_TITLES = [
    "Установить Microsoft Office",
    "Создать нового пользователя",
    "Обновить Windows до последней версии",
    "Настроить VPN доступ",
    "Добавить принтер в сеть",
    "Установить антивирус",
    "Предоставить доступ к папке",
    "Настроить корпоративную почту",
    "Установить VS Code",
    "Обновить драйверы видеокарты",
    "Настроить резервное копирование",
    "Установить Zoom",
    "Создать общий диск",
    "Настроить удалённый доступ",
    "Установить 1С",
    "Обновить прошивку роутера",
    "Настроить двухфакторную аутентификацию",
    "Установить Docker Desktop",
    "Добавить пользователя в группу",
    "Настроить почтовый клиент Outlook",
]

DESCRIPTIONS = [
    "Пользователь сообщает о проблеме. Требуется срочное решение.",
    "Обращение поступило от сотрудника отдела бухгалтерии.",
    "Проблема возникла после последнего обновления системы.",
    "Пользователь не может продолжать работу до решения вопроса.",
    "Обращение поступило повторно, предыдущее решение не помогло.",
    "Проблема наблюдается на нескольких рабочих станциях.",
    "Требуется установка стандартного программного обеспечения.",
    "Запрос согласован с руководителем отдела.",
    "Необходима настройка в соответствии с корпоративными стандартами.",
    "Пользователь ожидает выполнения в течение рабочего дня.",
]


def weighted_choice(distribution: dict) -> int:
    keys = list(distribution.keys())
    weights = list(distribution.values())
    return random.choices(keys, weights=weights, k=1)[0]


def random_date_in_past(days_back: int) -> str:
    """Генерирует случайную дату-время за последние days_back дней."""
    now = datetime.now()
    random_days = random.randint(0, days_back)
    random_seconds = random.randint(0, 86400)  # случайное время суток
    dt = now - timedelta(days=random_days, seconds=random_seconds)
    # Формат который принимает GLPI
    return dt.strftime("%Y-%m-%d %H:%M:%S")


def init_session() -> str:
    response = requests.get(
        f"{GLPI_URL}/initSession",
        headers={
            "App-Token": APP_TOKEN,
            "Authorization": f"user_token {USER_TOKEN}",
        }
    )
    if response.status_code != 200:
        raise Exception(f"Ошибка авторизации: {response.status_code} — {response.text}")
    return response.json()["session_token"]


def kill_session(session_token: str):
    requests.get(
        f"{GLPI_URL}/killSession",
        headers={
            "App-Token": APP_TOKEN,
            "Session-Token": session_token,
        }
    )


def create_ticket(session_token: str, title: str, description: str,
                  ticket_type: int, priority: int, date: str) -> int:
    response = requests.post(
        f"{GLPI_URL}/Ticket",
        headers={
            "App-Token": APP_TOKEN,
            "Session-Token": session_token,
            "Content-Type": "application/json",
        },
        json={
            "input": {
                "name": title,
                "content": description,
                "type": ticket_type,
                "priority": priority,
                "status": random.choice([1, 2, 5, 6]),  # Новая/В работе/Решена/Закрыта
                "date": date,           # дата открытия
                "date_creation": date,  # дата создания
            }
        }
    )
    if response.status_code not in (200, 201):
        raise Exception(f"Ошибка: {response.status_code} — {response.text}")
    return response.json()["id"]


def main():
    print("=" * 50)
    print(f"Создание {TOTAL_TICKETS} заявок за последние {DAYS_BACK} дней")
    print(f"URL: {GLPI_URL}")
    print("=" * 50)

    print("\nПодключение к GLPI...")
    try:
        session_token = init_session()
        print("Авторизация успешна!\n")
    except Exception as e:
        print(f"Ошибка: {e}")
        return

    created = 0
    failed = 0

    try:
        for i in range(1, TOTAL_TICKETS + 1):
            ticket_type = weighted_choice(TYPE_DISTRIBUTION)
            title = random.choice(
                INCIDENT_TITLES if ticket_type == 1 else REQUEST_TITLES
            ) + f" #{i}"
            description = random.choice(DESCRIPTIONS)
            priority = weighted_choice(PRIORITY_DISTRIBUTION)
            date = random_date_in_past(DAYS_BACK)

            try:
                ticket_id = create_ticket(
                    session_token, title, description,
                    ticket_type, priority, date
                )
                created += 1

                if i % 50 == 0 or i == TOTAL_TICKETS:
                    percent = (i / TOTAL_TICKETS) * 100
                    print(f"[{percent:5.1f}%] Создано: {created} | Ошибок: {failed} | ID: {ticket_id} | Дата: {date}")

            except Exception as e:
                failed += 1
                print(f"  Ошибка при заявке #{i}: {e}")

            if DELAY > 0:
                time.sleep(DELAY)

    finally:
        kill_session(session_token)
        print("\n" + "=" * 50)
        print(f"Готово! Создано: {created} | Ошибок: {failed}")
        print(f"Теперь нажми «Синхронизировать» на localhost:3000")
        print("=" * 50)


if __name__ == "__main__":
    main()
