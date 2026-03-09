using System;
using UnityEngine;
using BepInEx;
using HarmonyLib;
using Photon.Voice;
using System.Collections.Generic;
using BepInEx.Configuration;
using GorillaNetworking;

namespace Xon
{
    [BepInPlugin("ProxoMenuTemplate", "Proxo", "1.0.0")]
    public class Main : BaseUnityPlugin
    {// Please give credit if you use this as a template, it took a lot of time to make. Also, if you have any questions or need help, feel free to ask me on discord
        // copyRigtht 2026

        bool isMenuCreated;
        GameObject menuObj;
        List<GameObject> btnObjs = new List<GameObject>();
        public float pageSwitcherCooldown = 0f;

        public int currenetCategoryIndex = -1;
        public int CurrentPageIndex = 0;

        List<List<List<string>>> allCategories = new List<List<List<string>>>();
        List<string> categoryNames = new List<string> { "Movement", "Extras" };

        public static Main instance;

        public ConfigEntry<bool> speedBoostEnabled;
        public ConfigEntry<bool> flyEnabled;
        public ConfigEntry<bool> GhostMonkEnabled;
        void Awake()
        {
            instance = this;

            speedBoostEnabled = Config.Bind("Settings", "SpeedBoost", false, "gives player speed");
            flyEnabled = Config.Bind("Settings", "fly", false, "gives player ability to fly");
            GhostMonkEnabled = Config.Bind("Settings", "GhostMonk", false, "in the name");

            List<List<String>> movmentPages = new List<List<String>> {

            new List<string> { "SpeedBoost", "fly" },
            new List<string> { "GhostMonk", "fly" },
             

            };

            List<List<string>> extraPages = new List<List<string>>
            {
                new List<string> {"Quit", "Disconnect"}
            };

            allCategories.Add(movmentPages);
            allCategories.Add(extraPages);

            Harmony harmony = new Harmony("ProxoMenuTemplate");
            harmony.PatchAll();
        }

        void Start()
        {

        }

        void Update()
        {
            if (!isMenuCreated && ControllerInputPoller.instance.leftControllerSecondaryButton)
            {
                CreateMenu();
            }
            else if (isMenuCreated && !ControllerInputPoller.instance.leftControllerSecondaryButton)
            {
                DestroyMenu();
            }

            if (speedBoostEnabled.Value) Mods.SpeedBoost();
            if (flyEnabled.Value) Mods.fly();
            if (GhostMonkEnabled.Value) Mods.GhostMonk();
        }

        void CreateMenu()
        {
            isMenuCreated = true;

            var player = GorillaLocomotion.GTPlayer.Instance;

            menuObj = GameObject.CreatePrimitive(PrimitiveType.Cube);
            menuObj.transform.parent = player.LeftHand.controllerTransform;
            menuObj.transform.localPosition = Vector3.zero;
            menuObj.transform.localRotation = Quaternion.identity;
            menuObj.transform.localScale = new Vector3(0.03f, 0.3f, 0.45f);

            Destroy(menuObj.GetComponent<Rigidbody>());
            Destroy(menuObj.GetComponent<Collider>());

            var rend = menuObj.GetComponent<Renderer>();
            rend.material.shader = Shader.Find("GorillaTag/UberShader");
            rend.material.color = Color.gray1;

            AddButton(0.15f, "SpeedBoost");
            AddButton(0.1f, "Fly");
            AddButton(0.05f, "GhostMonk");
        }
        void DestroyMenu()
        {
            isMenuCreated = false;
            if (menuObj != null) Destroy(menuObj);
            DestroyAllButton();
            Main.instance.Config.Save();
        }

        void AddButton(float zOffset, string BtnName)
        {
            GameObject btnObj = GameObject.CreatePrimitive(PrimitiveType.Cube);

            var player = GorillaLocomotion.GTPlayer.Instance;

            var follow = btnObj.AddComponent<FollowMenu>();
            follow.target = player.LeftHand.controllerTransform;
            follow.position = new Vector3(0.051f, 0f, zOffset);
            follow.rotation = Quaternion.identity;
            //btnObj.transform.localPosition = new Vector3(-0.5f, 0, 0);

            btnObj.transform.localScale = new Vector3(0.03f, 0.2f, 0.04f);

            var rend = btnObj.GetComponent<Renderer>();
            rend.material.shader = Shader.Find("GorillaTag/UberShader");
            rend.material.color = Color.softBlue;

            btnObj.GetComponent<Collider>().isTrigger = true;
            btnObj.layer = 2;

            var trigger = btnObj.AddComponent<ButtonTrigger>();
            trigger.btnid = BtnName;

            btnObjs.Add(btnObj);
        }

        void DestroyAllButton()
        {
            foreach (GameObject btnObj in btnObjs)
            {
                if (btnObj != null) Destroy(btnObj);
            }
            btnObjs.Clear();
        }
    }

    public class FollowMenu : MonoBehaviour
    {
        public Transform target;
        public Vector3 position;
        public Quaternion rotation;

        void LateUpdate()
        {
            if (target == null) return;
            transform.position = target.TransformPoint(position);
            transform.rotation = target.rotation * rotation;
        }
    }

    public class ButtonTrigger : GorillaPressableButton
    {
        public string btnid;
        bool isToggled;
        bool isTogglable;

        void start()
        {
            switch (btnid)
            {
                case "SpeedBoost":
                    isTogglable = true;
                    isToggled = Main.instance.speedBoostEnabled.Value;
                    break;

                case "Button2":
                    isTogglable = true;
                    isToggled = Main.instance.flyEnabled.Value;
                    break;

                case "GhostMonk":
                    isTogglable = true;
                    isToggled = Main.instance.GhostMonkEnabled.Value;
                    break;
            }
            if (isTogglable)
            {
                GetComponent<Renderer>().material.color = isToggled ? Color.green : Color.softRed;
            }
            else
            {
                GetComponent<Renderer>().material.color = Color.gray2;
            }
        }
        public override void ButtonActivationWithHand(bool isLeftHand)
        {
            base.ButtonActivationWithHand(isLeftHand);

            if (!isLeftHand)
            {
                if (isTogglable)
                {
                    isToggled = isToggled;
                    GetComponent<Renderer>().material.color = isToggled ? Color.green : Color.softRed;
                }
                switch (btnid)
                {
                    case "SpeedBoost":
                        Main.instance.speedBoostEnabled.Value = isToggled;
                        break;

                    case "Fly":
                        Main.instance.flyEnabled.Value = isToggled;
                        break;
                    case "GhostMonk":
                        Main.instance.GhostMonkEnabled.Value = isToggled;
                        break;
                }

                Main.instance.Config.Save();
            }
        }
    }
}