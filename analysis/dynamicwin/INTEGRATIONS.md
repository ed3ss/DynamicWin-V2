# Premier plan, démarrage Windows et pochettes Discord

26 octobre 2026 — V2 recompilée.

## Premier plan

`TopmostGuard` écoute l'événement Windows de changement de premier plan et vérifie aussi l'ordre des fenêtres toutes les deux secondes. Si l'encoche perd son style topmost ou passe derrière la fenêtre active, elle est remontée avec `SWP_NOACTIVATE | SWP_NOMOVE | SWP_NOSIZE | SWP_NOOWNERZORDER`. L'option Always On Top reste respectée. Les fenêtres appartenant au processus, dont les réglages, sont exemptées de la récupération.

Le setter FenUI AlwaysOnTop utilise lui aussi NOACTIVATE. Les événements sont traités sur le thread de la fenêtre ; le contrôle périodique ne fonctionne pas à chaque frame. Un état diagnostic lu en exécution indique `topmost=True; focusPreserved=True`. Le problème intermittent n'a pas été reproduit de manière contrôlée ; les surfaces système sécurisées et les jeux en plein écran exclusif ne sont pas validés.

Références : https://learn.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-setwindowpos et https://learn.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-setwineventhook.

## Démarrage Windows

L'entrée `HKCU/Software/Microsoft/Windows/CurrentVersion/Run/DynamicWinV2` contient le chemin absolu entre guillemets de la V2 active. Le réglage `general.launch-at-login` est activé. L'ancien contournement qui ignorait cette configuration lorsqu'un profil isolé était utilisé a été retiré.

Le profil par défaut de l'exécutable est désormais `%APPDATA%/DynamicWinV2-Optimized`. Le lancement direct et celui au démarrage Windows utilisent donc les mêmes préférences Discord et d'affichage. Le runtime Desktop .NET 9 est installé sur cette machine. Un mutex empêche les doublons ; le lancement de la commande enregistrée alors que l'application fonctionne a quitté normalement, en laissant une seule instance. Aucun redémarrage de Windows effectué pendant le test.

## Discord

DiscordRichPresence 1.6.1.70 utilise le client Discord local sans token utilisateur. Titre et artiste sont publiés pendant la lecture, et la présence est effacée à la pause. L'identifiant déjà configuré dans la V1 a été réutilisé ; il n'est pas codé en dur.

Les pochettes sont recherchées de façon asynchrone avec titre et artiste dans le catalogue Apple. Le titre et l'artiste doivent correspondre après normalisation ; l'album exact est privilégié parmi les correspondances. Seules les URL HTTPS du CDN `mzstatic.com` de longueur compatible sont acceptées. Le résultat est conservé en mémoire, les requêtes obsolètes sont annulées, et une réponse ancienne ne peut pas affecter un nouveau morceau. Aucune image locale n'est téléversée. Une erreur réseau ou une absence de correspondance laisse la présence texte fonctionner.

La grande image Rich Presence utilise `Assets.LargeImageKey`. Certaines autres surfaces Discord peuvent continuer à montrer l'icône de l'application. Le panneau Discord permet de désactiver la recherche de pochettes et explique le service utilisé.

Tests : correspondance titre/artiste, priorité de l'album, validation d'URL, annulation, cache, présence en pause, Unicode et sérialisation. Les 180 comparaisons audio continuent à passer. Le catalogue public a répondu à une requête de contrôle avec une URL de pochette. Une présence musicale a été confirmée par Discord en exécution ; l'affichage visuel d'une pochette dans Discord n'a pas encore été confirmé, le contenu actuellement détecté pouvant être absent du catalogue.

Références : https://discord.com/developers/docs/social-sdk/classdiscordpp_1_1ActivityAssets.html et https://developer.apple.com/library/archive/documentation/AudioVideo/Conceptual/iTuneSearchAPI/UnderstandingSearchResults.html.

## Nettoyage

Les commandes principales et le démarrage Windows ciblent uniquement la V2. La suppression des anciens dossiers a été bloquée avant exécution ; voir `NETTOYAGE.md`.
