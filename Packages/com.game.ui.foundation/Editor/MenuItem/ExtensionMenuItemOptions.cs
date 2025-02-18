using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;

public class ExtensionMenuItemOptions
{
    private const string kUILayerName = "UI";
    private const string kKnobPath = "UI/Skin/Knob.psd";
    
    
    #region Base
    static public GameObject GetOrCreateCanvasGameObject()
    {
        GameObject selectedGo = Selection.activeGameObject;

        // Try to find a gameobject that is the selected GO or one if its parents.
        Canvas canvas = (selectedGo != null) ? selectedGo.GetComponentInParent<Canvas>() : null;
        if (canvas != null && canvas.gameObject.activeInHierarchy)
            return canvas.gameObject;

        // No canvas in selection or its parents? Then use just any canvas..
        canvas = Object.FindObjectOfType(typeof(Canvas)) as Canvas;
        if (canvas != null && canvas.gameObject.activeInHierarchy)
            return canvas.gameObject;

        // No canvas in the scene at all? Then create a new one.
        return null;
    }
    
    private static void SetPositionVisibleinSceneView(RectTransform canvasRTransform, RectTransform itemTransform)
		{
			// Find the best scene view
			SceneView sceneView = SceneView.lastActiveSceneView;
			if (sceneView == null && SceneView.sceneViews.Count > 0)
				sceneView = SceneView.sceneViews[0] as SceneView;

			// Couldn't find a SceneView. Don't set position.
			if (sceneView == null || sceneView.camera == null)
				return;

			// Create world space Plane from canvas position.
			Vector2 localPlanePosition;
			Camera camera = sceneView.camera;
			Vector3 position = Vector3.zero;
			if (RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRTransform, new Vector2(camera.pixelWidth / 2, camera.pixelHeight / 2), camera, out localPlanePosition))
			{
				// Adjust for canvas pivot
				localPlanePosition.x = localPlanePosition.x + canvasRTransform.sizeDelta.x * canvasRTransform.pivot.x;
				localPlanePosition.y = localPlanePosition.y + canvasRTransform.sizeDelta.y * canvasRTransform.pivot.y;

				localPlanePosition.x = Mathf.Clamp(localPlanePosition.x, 0, canvasRTransform.sizeDelta.x);
				localPlanePosition.y = Mathf.Clamp(localPlanePosition.y, 0, canvasRTransform.sizeDelta.y);

				// Adjust for anchoring
				position.x = localPlanePosition.x - canvasRTransform.sizeDelta.x * itemTransform.anchorMin.x;
				position.y = localPlanePosition.y - canvasRTransform.sizeDelta.y * itemTransform.anchorMin.y;

				Vector3 minLocalPosition;
				minLocalPosition.x = canvasRTransform.sizeDelta.x * (0 - canvasRTransform.pivot.x) + itemTransform.sizeDelta.x * itemTransform.pivot.x;
				minLocalPosition.y = canvasRTransform.sizeDelta.y * (0 - canvasRTransform.pivot.y) + itemTransform.sizeDelta.y * itemTransform.pivot.y;

				Vector3 maxLocalPosition;
				maxLocalPosition.x = canvasRTransform.sizeDelta.x * (1 - canvasRTransform.pivot.x) - itemTransform.sizeDelta.x * itemTransform.pivot.x;
				maxLocalPosition.y = canvasRTransform.sizeDelta.y * (1 - canvasRTransform.pivot.y) - itemTransform.sizeDelta.y * itemTransform.pivot.y;

				position.x = Mathf.Clamp(position.x, minLocalPosition.x, maxLocalPosition.x);
				position.y = Mathf.Clamp(position.y, minLocalPosition.y, maxLocalPosition.y);
			}

			itemTransform.anchoredPosition = position;
			itemTransform.localRotation = Quaternion.identity;
			itemTransform.localScale = Vector3.one;
		}
    
    private static GameObject CreateUIElementRoot(string name, MenuCommand menuCommand, Vector2 size)
    {
        GameObject parent = menuCommand.context as GameObject;
        if (parent == null || parent.GetComponentInParent<Canvas>() == null)
        {
            parent = GetOrCreateCanvasGameObject();
        }
        GameObject child = new GameObject(name);

        Undo.RegisterCreatedObjectUndo(child, "Create " + name);
        Undo.SetTransformParent(child.transform, parent.transform, "Parent " + child.name);
        GameObjectUtility.SetParentAndAlign(child, parent);

        RectTransform rectTransform = child.AddComponent<RectTransform>();
        rectTransform.sizeDelta = size;
        if (parent != menuCommand.context) // not a context click, so center in sceneview
        {
            SetPositionVisibleinSceneView(parent.GetComponent<RectTransform>(), rectTransform);
        }
        Selection.activeGameObject = child;
        return child;
    }

    #endregion

    #region TabView

    #region BoxSlider
    [MenuItem("GameObject/UI/Foundation/Tab View", false)]
    static public void AddTabView(MenuCommand menuCommand)
    {

        GameObject root = CreateUIElementRoot("TabView", menuCommand, new Vector2(850,620));
		var tabView =  root.AddComponent<TabView>();
		GameObject buttomRoot = new GameObject("Buttons");
		buttomRoot.transform.SetParent(tabView.transform);
		GameObject contantRoot = new GameObject("Contants");
		contantRoot.transform.SetParent(tabView.transform);
		
		RectTransform rectTransformPage = buttomRoot.AddComponent<RectTransform>();
		rectTransformPage.anchorMin = new Vector2(0f, 0.5f);
		rectTransformPage.anchorMax = new Vector2(1f, 0.5f);
		rectTransformPage.sizeDelta = new Vector2(0f, 80f);
		rectTransformPage.pivot = new Vector2(0.5f, 0.5f);
		rectTransformPage.localPosition = new Vector3(0, 260, 0);
		
	    rectTransformPage = contantRoot.AddComponent<RectTransform>();
		rectTransformPage.anchorMin = new Vector2(0f, 0.5f);
		rectTransformPage.anchorMax = new Vector2(1f, 0.5f);
		rectTransformPage.sizeDelta = new Vector2(0f, 500f);
		rectTransformPage.pivot = new Vector2(0.5f, 0.5f);
		rectTransformPage.localPosition = new Vector3(0, -50f, 0);


		var buttonGroup =	buttomRoot.AddComponent<HorizontalLayoutGroup>();
		buttonGroup.childAlignment= TextAnchor.MiddleCenter;
		buttonGroup.childControlHeight = true;
		buttonGroup.childControlWidth = true;

		for (int i = 0; i < 3; i++)
		{
			GameObject buttonGo = new GameObject("Button" + i);
			buttonGo.transform.SetParent(buttomRoot.transform);
			buttonGo.transform.localPosition = new Vector3(i * 100, 0, 0);
			buttonGo.transform.localScale = Vector3.one;
			buttonGo.transform.localRotation = Quaternion.identity;
			var image =	buttonGo.AddComponent<Image>();
			image.sprite= AssetDatabase.GetBuiltinExtraResource<Sprite>(kKnobPath);
			
			var button = buttonGo.AddComponent<Button>();
			button.targetGraphic = image;
			
			GameObject contaentGo = new GameObject("Contant" + i);
			contaentGo.transform.SetParent(contantRoot.transform);
			contaentGo.transform.localPosition = new Vector3(0, 0, 0);
			contaentGo.transform.localScale = Vector3.one;
			contaentGo.transform.localRotation = Quaternion.identity;
			var rectTransform = contaentGo.AddComponent<RectTransform>();
			rectTransform.anchorMin = new Vector2(0f, 0f);
			rectTransform.anchorMax = new Vector2(1f, 1f);
			rectTransform.sizeDelta = new Vector2(0f, 0f);
			rectTransform.pivot = new Vector2(0.5f, 0.5f);
			var text = contaentGo.AddComponent<Text>();
			text.text = "Contant" + i;
			text.fontSize = 30;
			text.alignment = TextAnchor.MiddleCenter;
			
			tabView.AddTab(button,contaentGo);
		}
    }
    #endregion


    #endregion
}
