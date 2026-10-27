# Correctifs de stabilité — 27 octobre 2026

Les deux diagnostics Windows du 26 octobre montrent une corruption native : le premier dans une finalisation WinRT, le second dans la libération d’un snapshot Skia par CachedSurface.Draw. Ces piles indiquent où le défaut est détecté, sans prouver à elles seules le point de corruption initial.

Corrections apportées :
- Surfaces graphiques du cache allouées et possédées par Skia ; les snapshots conservent leur texture. Suppression des textures D3D externes libérées trop tôt.
- Un seul propriétaire des ressources lors de la reconstruction des surfaces. Les snapshots sont libérés avant leur surface.
- Copie de capture écran ligne par ligne, sans copier le remplissage GPU dans un tampon plus petit. Équilibrage Map/Unmap, ReleaseFrame et libération des interfaces DirectX.
- Transmission bornée des captures au fil de rendu, libération des images remplacées et des shaders temporaires.
- Sérialisation des mises à jour de miniatures, suppression des trois chargements concurrents et libération des bitmaps temporaires. Animations des pochettes mises à jour via le dispatcher.
- Nettoyage des raccourcis de SongCover sans demander un contexte FenUI sur le mauvais fil.
- Recherche de pochette Deezer en secours après iTunes, avec vérification exacte du titre et de l’artiste et URL HTTPS autorisée uniquement.

Validation : compilation des copies locale et publique, 180 comparaisons audio, tests Discord/pochettes et 15 cas de copie mémoire avec marges de garde intactes. La version relancée a fonctionné plusieurs minutes et Discord a confirmé la présence avec pochette. Aucun test court ne garantit l’absence de tout crash ; les diagnostics Windows restent utiles en cas de récidive.

Les dumps et journaux contenant des chemins locaux restent hors du dépôt.

## Protection du focus en jeu

Quick Drop ne démarre que pour un geste commencé dans l’Explorateur Windows ; sa fenêtre de détection ne demande plus le focus. L’overlay se masque pendant que Fortnite est au premier plan (y compris en fenêtre), ou qu’une autre fenêtre couvre son moniteur, puis se restaure avec SW_SHOWNA. La préférence « toujours au premier plan » est conservée.

Validation : compilation des deux distributions, tests de géométrie plein écran/écran secondaire/fenêtre maximisée/fenêtre normale, et suite de tests existante. Le comportement en partie réelle reste à confirmer.
