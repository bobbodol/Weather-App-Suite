"""
weather.py - Погодное приложение на Python (CLI + Tkinter опционально)
Для работы требуется requests и установленный API ключ в переменной окружения OPENWEATHER_API_KEY
"""

import os
import json
import sys
from datetime import datetime
import requests
from collections import defaultdict

API_KEY = os.environ.get("OPENWEATHER_API_KEY", "")
if not API_KEY:
    print("❌ Ошибка: установите переменную окружения OPENWEATHER_API_KEY")
    sys.exit(1)

BASE_URL = "https://api.openweathermap.org/data/2.5"

def get_weather(city, units="metric"):
    """Получить текущую погоду и прогноз"""
    # текущая погода
    current_url = f"{BASE_URL}/weather?q={city}&appid={API_KEY}&units={units}&lang=ru"
    current_resp = requests.get(current_url)
    if current_resp.status_code != 200:
        return None, None, current_resp.status_code
    current = current_resp.json()
    
    # прогноз на 5 дней
    forecast_url = f"{BASE_URL}/forecast?q={city}&appid={API_KEY}&units={units}&lang=ru"
    forecast_resp = requests.get(forecast_url)
    if forecast_resp.status_code != 200:
        forecast = None
    else:
        forecast = forecast_resp.json()
    return current, forecast, 200

def display_current(current, units):
    temp_unit = "°C" if units == "metric" else "°F"
    wind_unit = "m/s" if units == "metric" else "mph"
    temp = current['main']['temp']
    feels_like = current['main']['feels_like']
    humidity = current['main']['humidity']
    wind = current['wind']['speed']
    pressure = current['main']['pressure']
    weather_desc = current['weather'][0]['description']
    city = current['name']
    
    print(f"\n📍 {city}")
    print(f"🌡️ Температура: {temp}{temp_unit} (ощущается как {feels_like}{temp_unit})")
    print(f"💧 Влажность: {humidity}%")
    print(f"💨 Ветер: {wind} {wind_unit}")
    print(f"🔽 Давление: {pressure} гПа")
    print(f"☁️ {weather_desc.capitalize()}")

def display_forecast(forecast, units):
    if not forecast:
        print("\n⚠️ Прогноз на 5 дней недоступен")
        return
    temp_unit = "°C" if units == "metric" else "°F"
    # группировка по дням
    daily = defaultdict(list)
    for item in forecast['list']:
        dt = datetime.fromtimestamp(item['dt'])
        day = dt.strftime("%Y-%m-%d")
        daily[day].append(item['main']['temp'])
    
    print("\n📅 Прогноз на 5 дней (макс/мин):")
    for day, temps in list(daily.items())[:5]:
        max_temp = max(temps)
        min_temp = min(temps)
        print(f"{day}: {min_temp:.1f}{temp_unit} .. {max_temp:.1f}{temp_unit}")

def main():
    print("🌤️ Погодное приложение (Python)")
    units = input("Выберите единицы (1-метрические °C, 2-имперские °F): ").strip()
    units = "imperial" if units == "2" else "metric"
    while True:
        city = input("\nВведите название города (или 'q' для выхода): ").strip()
        if city.lower() == 'q':
            break
        current, forecast, status = get_weather(city, units)
        if status == 200:
            display_current(current, units)
            display_forecast(forecast, units)
        elif status == 401:
            print("Ошибка API: неверный ключ")
            break
        elif status == 404:
            print("Город не найден")
        else:
            print(f"Ошибка {status}")

if __name__ == "__main__":
    main()
