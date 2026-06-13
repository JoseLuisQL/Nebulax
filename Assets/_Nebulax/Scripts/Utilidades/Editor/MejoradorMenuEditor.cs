using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;

namespace Nebulax.EditorScripts
{
    public class MejoradorMenuEditor : EditorWindow
    {
        [MenuItem("Nebulax/Mejorar Menú Principal")]
        public static void MejorarMenu()
        {
            // 1. Encontrar el GestorMenuPrincipal
            GestorMenuPrincipal gestorMenu = FindObjectOfType<GestorMenuPrincipal>(true);
            if (gestorMenu == null)
            {
                Debug.LogError("No se encontró GestorMenuPrincipal en la escena activa.");
                return;
            }

            GameObject menuGo = gestorMenu.gameObject;

            // Refrescar para asegurar que Unity vea el archivo
            AssetDatabase.Refresh();

            // 2. Cargar la textura generada
            Texture2D texFondo = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/_Nebulax/Arte/FondoMenu.png");
            if (texFondo != null)
            {
                // Convertirla a Sprite si no lo es
                string assetPath = AssetDatabase.GetAssetPath(texFondo);
                TextureImporter importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;
                if (importer != null && importer.textureType != TextureImporterType.Sprite)
                {
                    importer.textureType = TextureImporterType.Sprite;
                    importer.SaveAndReimport();
                }

                Sprite spriteFondo = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/_Nebulax/Arte/FondoMenu.png");

                // Buscar la imagen de fondo en el menú o crear una
                Image imgFondo = null;
                Transform fondoTransform = menuGo.transform.Find("Fondo") ?? menuGo.transform.Find("Background") ?? menuGo.transform.Find("FondoMenu");
                
                if (fondoTransform != null)
                {
                    imgFondo = fondoTransform.GetComponent<Image>();
                }
                
                if (imgFondo == null)
                {
                    // Crear objeto de fondo específicamente en lugar de usar el panel principal
                    // Esto evita colorear todo el panel si es un layout.
                    GameObject fondoObj = new GameObject("FondoMenu");
                    fondoObj.transform.SetParent(menuGo.transform, false);
                    fondoObj.transform.SetAsFirstSibling(); // Mandar al fondo
                    imgFondo = fondoObj.AddComponent<Image>();
                    
                    RectTransform rect = imgFondo.rectTransform;
                    rect.anchorMin = Vector2.zero;
                    rect.anchorMax = Vector2.one;
                    rect.offsetMin = Vector2.zero;
                    rect.offsetMax = Vector2.zero;
                    rect.sizeDelta = Vector2.zero;
                }

                if (imgFondo != null && spriteFondo != null)
                {
                    imgFondo.sprite = spriteFondo;
                    imgFondo.color = Color.white;
                    imgFondo.type = Image.Type.Simple;
                    imgFondo.preserveAspect = false;
                }
            }
            else
            {
                Debug.LogWarning("No se encontró la textura en Assets/_Nebulax/Arte/FondoMenu.png");
            }
            // 3. Mejorar los botones y textos
            Button[] botones = menuGo.GetComponentsInChildren<Button>(true);
            foreach (Button btn in botones)
            {
                Image btnImg = btn.GetComponent<Image>();
                if (btnImg != null)
                {
                    // Hacer el botón semi-transparente oscuro
                    btnImg.color = new Color(0.02f, 0.08f, 0.25f, 0.9f);
                    
                    // Asegurarnos de que tiene un borde brillante usando Outline
                    Outline outline = btn.gameObject.GetComponent<Outline>();
                    if (outline == null) outline = btn.gameObject.AddComponent<Outline>();
                    outline.effectColor = new Color(0.2f, 0.8f, 1f, 1f); // Cian
                    outline.effectDistance = new Vector2(2, -2);
                    
                    // Añadir sombra para que resalte
                    Shadow shadowBtn = btn.gameObject.GetComponent<Shadow>();
                    if (shadowBtn == null) shadowBtn = btn.gameObject.AddComponent<Shadow>();
                    shadowBtn.effectColor = new Color(0, 0, 0, 0.8f);
                    shadowBtn.effectDistance = new Vector2(4, -4);
                }

                // Añadir el script de animación si no lo tiene
                BotonAnimado anim = btn.GetComponent<BotonAnimado>();
                if (anim == null)
                {
                    anim = btn.gameObject.AddComponent<BotonAnimado>();
                }

                Text txt = btn.GetComponentInChildren<Text>(true);
                if (txt != null)
                {
                    txt.color = Color.white;
                    txt.fontStyle = FontStyle.Bold;
                    txt.fontSize = 32;
                    txt.text = txt.text.ToUpper().Replace(" ", "  "); // Expandir el texto un poco (tracking falso)

                    Shadow shadowTxt = txt.gameObject.GetComponent<Shadow>();
                    if (shadowTxt == null) shadowTxt = txt.gameObject.AddComponent<Shadow>();
                    shadowTxt.effectColor = new Color(0, 0.5f, 1f, 0.8f); // Sombra azul brillante
                    shadowTxt.effectDistance = new Vector2(2, -2);
                }
                
                TMP_Text tmpTxt = btn.GetComponentInChildren<TMP_Text>(true);
                if (tmpTxt != null)
                {
                    tmpTxt.color = Color.white;
                    tmpTxt.fontStyle = FontStyles.Bold;
                    tmpTxt.fontSize = 32;
                    // tmpTxt soporta character spacing:
                    tmpTxt.characterSpacing = 10;
                }
            }

            // 4. Mejorar el Título si existe
            Text[] textos = menuGo.GetComponentsInChildren<Text>(true);
            foreach (Text t in textos)
            {
                if (t.name.ToLower().Contains("titul") || t.name.ToLower().Contains("title") || t.fontSize > 40)
                {
                    t.color = new Color(0.3f, 0.8f, 1f); // Cian brillante
                    t.fontSize = 72;
                    Shadow shadow = t.gameObject.GetComponent<Shadow>();
                    if (shadow == null) shadow = t.gameObject.AddComponent<Shadow>();
                    shadow.effectColor = new Color(0, 0, 0, 0.8f);
                    shadow.effectDistance = new Vector2(3, -3);
                }
            }
            
            Debug.Log("¡Menú Principal Mejorado con éxito!");
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
        }
    }
}
