# DynamicWin V2 — version modifiée

Projet non officiel basé sur DynamicWin v2.0.3 de Florian Butz : https://github.com/FlorianButz/DynamicWin
Sources reconstruites par décompilation et modifiées. Aucune affiliation avec l’auteur original.

Modifications : réduction des allocations du visualiseur audio, attente des frames sans boucle active, protection du premier plan, démarrage Windows et présence musicale Discord avec pochettes optionnelles.

## Compilation

Windows x64, PowerShell 7 et SDK .NET 9 requis. Depuis la racine :

```powershell
./build.ps1 -Test
```

Le script télécharge la distribution officielle v2.0.3 et vérifie son SHA-256 pour récupérer les dépendances. Le résultat est dans `dist/DynamicWinV2-Optimized`. Lancez `DynamicWinV2.exe`.

## Configuration personnelle

Aucun identifiant d’application Discord n’est fourni. Créez votre propre application Discord et saisissez son identifiant dans les paramètres Discord de DynamicWin. Les paramètres utilisateur restent dans `%APPDATA%/DynamicWinV2-Optimized`, hors du dépôt.

Les pochettes sont recherchées sur iTunes puis sur Deezer en secours à partir du titre et de l’artiste lorsque l’option est activée ; une correspondance n’est pas garantie.

Google Calendar nécessite votre propre fichier OAuth client JSON, désigné par la variable d’environnement `DYNAMICWIN_GOOGLE_CREDENTIALS`. Les identifiants intégrés à la distribution originale sont exclus des sources. Ne commitez aucun fichier OAuth ni le dossier `gcalendar`.

## Attribution et licence

DynamicWin et fenUI : Florian Butz et leurs contributeurs. Ce dépôt conserve la licence CC BY-SA 4.0 fournie avec DynamicWin (`LICENSE`), l’attribution et les notices des composants tiers. Les changements décrits ci-dessus sont propres à cette version modifiée. Les bibliothèques tierces restent soumises à leurs licences respectives.

Les sources décompilées produisent encore des avertissements de nullabilité. Les microbenchmarks ne prouvent pas à eux seuls une baisse globale de consommation CPU.

Les correctifs et leurs limites de validation sont décrits dans [STABILITE.md](analysis/dynamicwin/STABILITE.md).
