# OpenLP to ProPresenter Converter

This tool converts worship songs exported from OpenLP (in OpenLyrics `.xml` format) into files compatible with ProPresenter 7.

## 📥 How to Use

1. **Export Songs from OpenLP**  
   In OpenLP, go to **File > Export > Songs** and choose the **OpenLyrics (.xml)** format.

2. **Place Files in Input Folder**  
   Move all `.xml` files into the `input_folder`.

3. **Run the Converter**  
   Execute the script with:

   ```bash
   python3 converter.py input_folder output_folder
   ```

   The converted `.pro` files will be saved in the `output_folder`.

## 📄 Output Format

Each `.pro` file will contain verses labeled as `[Verse 1]`, `[Verse 2]`, etc., following the `<verseOrder>` defined in the original XML.  
If no `<verseOrder>` is specified, a default order will be generated automatically based on the appearance of the verses.

### Example Output:

```
[Verse 1]
Siamo per grazia davanti a Te,
Nella Tua CAsa, o Re dei re,
per ascoltare la verità
che la Tua bocca ci parlerà

[Verse 2]
Aprici a tutti la mente e il cuor,
e il nostro orecchio inchina, Signor,
alla parola di verità
che la Tua mano ci largirà.

[Verse 3]
Signore, dacci, per la virtù,
tutte le cose grate a Gesù.
Dacci la pace, la santità,
il Tuo timore, la carità.

[Verse 4]
In questo culto presiedi Tu,
guidalo a gloria del buon Gesù.
Con la Tua mano tocca ogni cuor,
che a Te si spande, Padre d'amor.
```

## 📤 Importing into ProPresenter 7

1. Open **ProPresenter 7**.
2. Go to **File > Import > Import Files...**
3. Select all the `.pro` files in the output folder (`Ctrl + A` on Windows or `Cmd + A` on macOS).
4. In the import window, set **Paragraph Break to 1**.
5. Confirm the import.

Your songs will now appear in the ProPresenter library, ready to be added to any playlist or service.

---

For issues or suggestions, feel free to open an issue in this repository.
