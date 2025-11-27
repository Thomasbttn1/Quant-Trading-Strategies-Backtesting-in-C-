# Backtesting de Stratégies de Trading Quantitatif en C#

## Description

Ce projet implémente un framework de backtesting pour tester des stratégies de trading quantitatif sur des données historiques de marché. Il permet de :

- Charger des données de prix historiques (CSV)
- Générer des signaux de trading selon différentes stratégies
- Simuler l'exécution de transactions
- Calculer les performances (rendement, drawdown, ratio de Sharpe, taux de réussite)
- Backtester un portefeuille multi-actifs

## Stratégies implémentées

### Stratégie Value
Achète lorsque le prix actuel est inférieur au prix d'il y a 2 ans (prix bas), vend quand le prix est plus élevé.

### Stratégie Momentum
Basée sur l'accélération du prix. Achète quand le prix s'accélère à la hausse, vend quand il s'accélère à la baisse.

## Structure du projet

```
my_package/
├── Program.cs                 # Point d'entrée, exécute les backtests
├── DonneesMarche.cs          # Classe pour charger les données CSV
├── Cotation.cs               # Représente une cotation (date + OHLCV)
├── StrategieTrading.cs       # Classe abstraite pour les stratégies
├── StrategieValue.cs         # Implémentation de la stratégie Value
├── StrategieMomentum.cs      # Implémentation de la stratégie Momentum
├── SignalTrading.cs          # Représente un signal d'achat/vente
├── Transaction.cs            # Représente une transaction complète (entrée + sortie)
├── BackTester.cs             # Moteur de backtesting
├── PortefeuilleBacktester.cs # Backtesting sur portefeuille multi-actifs
└── Performance.cs            # Résumé des performances

my_package.Tests/
└── Tests unitaires (si présents)
```

## Prérequis

- .NET 6.0 Obligatoirement
- Fichiers CSV de données de marché (format : Date,Open,High,Low,Close,Volume)

## Installation et lancement

### 1. Cloner le projet

```bash
git clone https://github.com/Thomasbttn1/Quant-Trading-Strategies-Backtesting-in-C-.git
cd Quant-Trading-Strategies-Backtesting-in-C-
git checkout PedroMomentum
```

### 2. Restaurer les dépendances

```bash
cd my_package
dotnet restore
```

### 3. Lancer le projet

```bash
dotnet run
```

Cela exécutera les backtests sur les données disponibles et affichera les résultats en console.

### 4. Builder le projet

```bash
dotnet build
```

### 5. Lancer les tests

Si des tests sont présents, les exécuter avec :

```bash
dotnet test
```

Ou depuis la racine du projet :

```bash
cd ..
dotnet test
```

## Format des données

Les fichiers CSV doivent avoir le format suivant :

```
Date,Open,High,Low,Close,Volume
2023-01-01,100.5,101.2,100.1,100.9,1000000
2023-01-02,100.9,102.1,100.5,101.5,1100000
...
```

Formats de date supportés :
- `yyyy-MM-dd` (ex: 2023-01-01)
- `dd/MM/yyyy` (ex: 01/01/2023)
- `MM/dd/yyyy` (ex: 01/01/2023)
- `yyyy/M/d` (ex: 2023/1/1)
- `d/M/yyyy` (ex: 1/1/2023)

## Utilisation

### Backtest simple d'une stratégie

Le `Program.cs` contient des exemples complets. Voici comment utiliser le framework :

```csharp
// Charger les données
var donnees = new DonneesMarche();
donnees.ChargerDepuisCSV("data/KFC.csv");

// Créer une stratégie
var strategie = new StrategieValue();

// Lancer le backtest
var backtester = new Backtester(strategie, donnees);
backtester.Executer();

// Afficher les résultats
backtester.AfficherResultats();
```

### Backtest sur portefeuille multi-actifs

```csharp
var strategie = new StrategieMomentum();
var portefeuille = new PortefeuilleBacktester(strategie);

portefeuille.ChargerDonnees("data/", new List<string> { "KFC", "SBUX", "NFLX" });
portefeuille.Executer();
portefeuille.AfficherResultatsGlobaux();
```

## Indicateurs de performance

Chaque backtest calcule :

- **Rendement cumulatif** : Rendement total en % sur la période
- **Drawdown maximum** : Plus grande perte en % depuis un sommet
- **Taux de réussite** : % de transactions gagnantes
- **Ratio de Sharpe** : Rendement ajusté au risque (annualisé)

## Notes

- Les stratégies supportent les positions longues (achat/vente) et les positions courtes (short) selon la configuration
- Un délai minimum entre les signaux peut être configuré pour éviter un trading trop fréquent
- Le taux sans risque peut être ajusté pour le calcul du ratio de Sharpe (par défaut 2%)

