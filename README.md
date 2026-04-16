<div align="center">
  <img src="./assets/logo.png" width="120" alt="CryptoVault Logo" style="border-radius:24px; box-shadow: 0px 4px 12px rgba(240, 185, 11, 0.4);" />
  
  <h1>CryptoVault</h1>
  <p><b>Professional Cryptocurrency Portfolio Management Platform</b></p>

  <p>
    <img src="https://img.shields.io/badge/.NET-10.0-512BD4?logo=dotnet&logoColor=white" alt=".NET 10" />
    <img src="https://img.shields.io/badge/Blazor-Server-512BD4?logo=blazor&logoColor=white" alt="Blazor Server" />
    <img src="https://img.shields.io/badge/Entity%20Framework-Core-0078D4?logo=nuget&logoColor=white" alt="EF Core" />
    <img src="https://img.shields.io/badge/Database-SQLite-003B57?logo=sqlite&logoColor=white" alt="SQLite" />
    <img src="https://img.shields.io/badge/Data-Binance%20API-F3BA2F?logo=binance&logoColor=black" alt="Binance API" />
  </p>
</div>

---

## 📖 Overview

CryptoVault is a high-fidelity, real-time investment simulator and portfolio manager designed for cryptocurrency traders. Built on **.NET 10** using **Blazor Server** with **InteractiveServer** rendering, it provides a seamless, SPA-like trading environment with robust C# backend services. 

The application utilizes **Clean Architecture** principles and fetches real-time market data through the **Binance Public API** (REST & WebSocket Streams), ensuring microsecond accuracy without manual page refreshes.

## ✨ Key Features

- **Real-Time Market Tracking**: Live updates of asset prices via background polling and WebSocket streaming from Binance.
- **Dynamic Portfolio Dashboard**: Interactive visualizations of asset allocations and performance metrics using customized `Chart.js` components.
- **High-Fidelity Trading Terminal**: Embedded **TradingView Lightweight Charts** integration for deep historical price analysis and OHLCV candlestick data tracking.
- **Watchlist & Analytics**: Seamless tools to track prospective pairs, analyze total investments, and visualize profit margins.
- **Premium Fintech UI**: Minimalist, dark-themed styling crafted entirely in vanilla CSS inspired by leading centralized exchanges.
- **No-Lag Background Sync**: A custom thread-safe background architecture avoids Blazor circuit disconnects by gracefully managing EF Core scoped instances during real-time updates.

## 🏗️ Architecture

CryptoVault follows a strictly layered **Clean Architecture** model:
1. **Domain Layer**: Core business entities (Assets, Portfolios, Transactions).
2. **Application Layer**: Business logic, DTOs, and Interfaces (IAssetService, IBinanceApiClient).
3. **Infrastructure Layer**: Concrete implementations of Entity Framework Core DbContexts and External API handlers.
4. **Presentation Layer**: Blazor Server UI Components, static assets, and scoped injection bindings.

## 🚀 Getting Started

### Prerequisites
- [.NET 10.0 SDK](https://dotnet.microsoft.com/download)
- Compatible IDE (Visual Studio 2022, Rider, or VS Code)

### Setup & Run
1. **Clone the repository:**
   ```bash
   git clone <repo-url>
   cd CryptoVault
   ```
2. **Restore Dependencies:**
   ```bash
   dotnet restore
   ```
3. **Apply Database Migrations:**
   ```bash
   dotnet ef database update
   ```
4. **Run the Server:**
   ```bash
   dotnet run
   ```
   Navigate to `https://localhost:7250` to view the platform.

## 🛠️ Technological Deep-Dive

- **Entity Framework Concurrency Management**: Highly complex implementation capturing real-time price updates while cleanly escaping `System.InvalidOperationException` pipeline crashes during `InvokeAsync` GUI manipulation.
- **Smart Asset Mapping**: Advanced token filtering fetching `IconUrlTemplate` images natively off of Binance's Static User-Content CDN (`bin.bnbstatic.com`).
- **Dynamic Native JS Interop**: Clean, decoupled interop connecting Blazor C# memory directly to localized `.js` canvas renderers.

---
> CryptoVault — Developed as a .NET Academic Project.
