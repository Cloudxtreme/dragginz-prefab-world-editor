﻿//
// Author  : Oliver Brodhage
// Company : Decentralised Team of Developers
//

using System;

using UnityEngine;
using UnityEngine.UI;

namespace PrefabWorldEditor
{
    public class UIInstanceInfo : MonoBehaviour
    {
        public Text assetName;

        public Toggle rotateX;
        public Toggle rotateY;
        public Toggle rotateZ;

        public Toggle gravity;

        public Slider sliderSnowLevel;

        public UIDynamicInstanceSettings dynamicSettings;

        // ---------------------------------------------------------------------------------------------
        public void init(PrefabLevelEditor.Part part, LevelController.LevelElement element) {

            if (name != null) {
                assetName.text = part.name;
            }

            if (rotateX != null) {
                rotateX.isOn = (part.canRotate.x == 1);
                rotateX.interactable = false;
            }
            if (rotateY != null) {
                rotateY.isOn = (part.canRotate.y == 1);
                rotateY.interactable = false;
            }
            if (rotateZ != null) {
                rotateZ.isOn = (part.canRotate.z == 1);
                rotateZ.interactable = false;
            }

            if (gravity != null) {
                if (element.go != null) {
                    gravity.isOn = (element.overwriteGravity == 0 ? part.usesGravity : (element.overwriteGravity == 1 ? true : false));
                }
                else {
                    gravity.isOn = part.usesGravity;
                }
                gravity.interactable = (part.type != Globals.AssetType.Floor && part.type != Globals.AssetType.Wall && part.type != Globals.AssetType.Dungeon);
            }

            if (sliderSnowLevel != null) {
                sliderSnowLevel.value = element.shaderSnow;
                sliderSnowLevel.transform.parent.gameObject.SetActive(LevelController.Instance.hasSnowShader);
            }

            // dynamic asset settings
            DynamicAsset dynAssetScript = element.go.GetComponent<DynamicAsset>();
            if (dynAssetScript != null) {
                showDynamicSettings(true);
                dynamicSettings.init(dynAssetScript.setupList);
            } else {
                showDynamicSettings (false);
            }
        }

        // ---------------------------------------------------------------------------------------------
        public void showDynamicSettings(bool state) {
            dynamicSettings.gameObject.SetActive(state);
        }

        // ---------------------------------------------------------------------------------------------
        public void onGravityValueChange(bool value) {

            if (LevelController.Instance.selectedElement.go != null) {

                LevelController.LevelElement e = LevelController.Instance.selectedElement;
                e.overwriteGravity = (gravity.isOn ? 1 : 2);
                LevelController.Instance.selectedElement = e;

                LevelController.Instance.saveSelectElement();
            }
        }

        // -------------------------------------------------------------------------------------
        public void onSliderSnowLevelChange(Single value) {

            LevelController.Instance.changeSnowLevel((float)value);

            LevelController.LevelElement e = LevelController.Instance.selectedElement;
            e.shaderSnow = (float)value;
            LevelController.Instance.selectedElement = e;

            LevelController.Instance.saveSelectElement();
        }
    }
}