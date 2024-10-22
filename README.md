# FRENCH
# README.md

Ce fork ajoute la prise en charge de nouveaux émulateurs pour la décompression des fichiers de jeux sous **RetroBat** et, éventuellement, sous **Batocera**, y compris les formats **.zip**, **.7z**, et **.squashfs**.

## Description

Je vais progressivement inclure d'autres émulateurs pour aider l'équipe originale, car je sais que ce sujet est demandé depuis longtemps. Je le fais de ma propre initiative, en supposant que l'équipe est probablement submergée par les demandes, et qu'ils ont déjà fait un énorme travail récemment sur la décompression. De mon côté, il ne reste qu'à ajouter de nouveaux émulateurs.

J'ai réussi mon premier test concluant avec l'émulateur **Xenia** : désormais, **RetroBat** prend en charge la décompression des fichiers **.zip** et **.7z** via Xenia pour les fichiers **ISO**, et même pour des fichiers **.zar** encapsulés dans des archives **.7z** ou **.zip**, afin de couvrir toutes les situations possibles, même les plus imprévues ;-). J'ai aussi ajouté la prise en charge des jeux **XBLA** compressés en **.7z** et **.zip** avec une arborescence classique, ce qui représente une avancée importante pour les launchers. Je suis actuellement en train de finaliser quelques tests sur les fichiers **.xex** pour ajouter une fonctionnalité supplémentaire, même si c'est un défi que je ne suis pas sûr de relever. Rassurez-vous, ma modification actuelle prend déjà en charge la lecture des fichiers **.xex**.

## Prise en charge du ZSTD

J'ai également intégré la prise en charge de la compression **ZSTD** de manière simple, en mettant à jour les DLL de **7-Zip**, qui n'étaient plus à jour depuis longtemps et provoquaient des problèmes de décompression. Pour cela, j'ai utilisé le fork **7-Zip ZSTD** standard. L'intérêt de **ZSTD** est particulièrement recommandé pour les très gros fichiers (à partir de **500 Mo**). 

Pour ceux qui souhaitent maintenir leurs sets de jeux, notamment ceux comprenant majoritairement des **BIN/CUE** et des **ISO**, il est recommandé de revoir vos sets et de compresser vos fichiers en **.7z** ou **.zip** avec l'algorithme de compression **ZSTD** pour bénéficier d'une rapidité de décompression optimale. Bien que **Deflate** fonctionne toujours, il peut entraîner des lenteurs. Les nouvelles **DLL** corrigent également certains bugs d'extraction liés aux fichiers compressés, même sous **Deflate**.

Vous pouvez facilement gérer vos archives avec le gestionnaire de ROM **RomVault**, qui intègre cette possibilité de conversion, plus ou moins rapide selon les sets.

> **Note :** Si vous utilisez toujours **Deflate**, soyez prêt à faire face à des ralentissements.

## Liens utiles

- **RomVault** : [https://www.romvault.com/](https://www.romvault.com/)
- **7-Zip ZSTD Fork** : [https://github.com/mcmilk/7-Zip-zstd](https://github.com/mcmilk/7-Zip-zstd)

---

## Remerciements

Je tiens à remercier les équipes de **RetroBat**, **Batocera**, et **EmulationStation** pour leur travail remarquable et leur dévouement à améliorer l'expérience utilisateur. Merci également au programmeur du fork **7-Zip ZSTD** et à tous ceux que j'ai pu oublier, qui ont contribué à rendre ces avancées possibles.


#
# ENGLISH
# README.md

This fork adds support for new emulators to decompress game files in **RetroBat** and, eventually, in **Batocera**, including the **.zip**, **.7z**, and **.squashfs** formats.

## Description

I will gradually add more emulators to assist the original team, as I know this feature has been requested for a long time. I'm doing this on my own initiative, assuming the team is probably overwhelmed with requests and they have already done an enormous job recently on decompression. On my side, it only remains to add support for new emulators.

I successfully completed my first test with the **Xenia** emulator: from now on, **RetroBat** supports decompression of **.zip** and **.7z** files via Xenia for **ISO** files, and even **.zar** files encapsulated within **.7z** or **.zip** archives to cover all possible situations, even the most unexpected ones ;-). I have also added support for **XBLA** games compressed in **.7z** and **.zip** with a classic directory structure, which represents an important advancement for launchers. I am currently finalizing some tests on **.xex** files to add extra functionality, although I am not sure I will be able to achieve it. Rest assured, the current modification already supports reading **.xex** files.

## ZSTD Support

I have also integrated support for **ZSTD** compression in a simple way, by updating the **7-Zip** DLLs, which had not been updated for a long time and were causing decompression problems. For this, I used the standard **7-Zip ZSTD** fork. The advantage of **ZSTD** is particularly recommended for very large files (from **500 MB** onwards).

For those who wish to maintain their game sets, especially those consisting mainly of **BIN/CUE** and **ISO** files, it is recommended to review your sets and compress your files in **.7z** or **.zip** using the **ZSTD** compression algorithm to achieve optimal decompression speed. Although **Deflate** still works, it may cause slowdowns. The new **DLLs** also fix some extraction bugs related to compressed files, even under **Deflate**.

You can easily manage your archives with the **RomVault** ROM manager, which includes this conversion capability, more or less quickly depending on the sets.

> **Note:** If you continue to use **Deflate**, be prepared to face slowdowns.

## Useful Links

- **RomVault**: [https://www.romvault.com/](https://www.romvault.com/)
- **7-Zip ZSTD Fork**: [https://github.com/mcmilk/7-Zip-zstd](https://github.com/mcmilk/7-Zip-zstd)

---

## Acknowledgements

I would like to thank the teams at **RetroBat**, **Batocera**, and **EmulationStation** for their remarkable work and dedication to improving the user experience. Thanks also to the programmer of the **7-Zip ZSTD** fork and everyone else I may have forgotten, who contributed to making these improvements possible.


#
# SPANISH
# README.md

Este fork agrega soporte para nuevos emuladores para la descompresión de archivos de juegos en **RetroBat** y, eventualmente, en **Batocera**, incluyendo los formatos **.zip**, **.7z**, y **.squashfs**.

## Descripción

Voy a incluir gradualmente más emuladores para ayudar al equipo original, ya que sé que esta función ha sido solicitada desde hace mucho tiempo. Estoy haciendo esto por mi cuenta, suponiendo que el equipo probablemente esté abrumado con solicitudes y que ya hayan hecho un trabajo enorme recientemente en la descompresión. Por mi parte, solo queda agregar soporte para nuevos emuladores.

He completado con éxito mi primera prueba con el emulador **Xenia**: a partir de ahora, **RetroBat** soporta la descompresión de archivos **.zip** y **.7z** a través de Xenia para archivos **ISO**, e incluso archivos **.zar** encapsulados en archivos **.7z** o **.zip** para cubrir todas las situaciones posibles, incluso las más inesperadas ;-). También he añadido soporte para juegos **XBLA** comprimidos en **.7z** y **.zip** con una estructura de directorios clásica, lo cual representa un avance importante para los launchers. Actualmente estoy finalizando algunas pruebas con los archivos **.xex** para agregar una funcionalidad adicional, aunque no estoy seguro de poder lograrlo. No obstante, la modificación actual ya soporta la lectura de archivos **.xex**.

## Soporte de ZSTD

También he integrado el soporte para la compresión **ZSTD** de manera sencilla, actualizando las DLL de **7-Zip**, que no se habían actualizado durante mucho tiempo y estaban causando problemas de descompresión. Para esto, utilicé el fork estándar de **7-Zip ZSTD**. La ventaja de **ZSTD** se recomienda particularmente para archivos muy grandes (a partir de **500 MB**).

Para aquellos que deseen mantener sus sets de juegos, especialmente aquellos que consisten principalmente en archivos **BIN/CUE** y **ISO**, se recomienda revisar sus sets y comprimir sus archivos en **.7z** o **.zip** utilizando el algoritmo de compresión **ZSTD** para lograr una velocidad de descompresión óptima. Aunque **Deflate** todavía funciona, puede causar lentitud. Las nuevas **DLL** también corrigen algunos errores de extracción relacionados con los archivos comprimidos, incluso con **Deflate**.

Puede gestionar fácilmente sus archivos con el gestor de ROM **RomVault**, que incluye esta capacidad de conversión, de manera más o menos rápida según los sets.

> **Nota:** Si continúa usando **Deflate**, prepárese para enfrentar ralentizaciones.

## Enlaces útiles

- **RomVault**: [https://www.romvault.com/](https://www.romvault.com/)
- **7-Zip ZSTD Fork**: [https://github.com/mcmilk/7-Zip-zstd](https://github.com/mcmilk/7-Zip-zstd)

---

## Agradecimientos

Quisiera agradecer a los equipos de **RetroBat**, **Batocera**, y **EmulationStation** por su notable trabajo y dedicación para mejorar la experiencia del usuario. También gracias al programador del fork **7-Zip ZSTD** y a todos los demás que pueda haber olvidado, quienes contribuyeron para que estas mejoras fueran posibles.


