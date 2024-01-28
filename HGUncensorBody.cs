using BepInEx;
using H;
using Character;
using HarmonyLib;
using System.Collections.Generic;
using UnityEngine;


namespace UncensorBody
{
    [BepInProcess("PlayHome32bit")]
    [BepInProcess("PlayHome64bit")]
    [BepInPlugin(PLUGIN_GUID, PLUGIN_NAME, PLUGIN_Version)]
    public class UncensorBody : BaseUnityPlugin
    {
        public const string PLUGIN_NAME = "Automatic Uncensor Body";
        public const string PLUGIN_GUID = "HG.UncensorBody";
        public const string PLUGIN_Version = "1.0.0";

        internal static string abDataPath = Application.dataPath.Substring(0, Application.dataPath.LastIndexOf('/')) + "/BepInEx/plugins/HG";
        internal static string abName = "HGUncensorBody";

        private static List<UBFemale> UBfemales = new List<UBFemale>();
        private static List<UBMale> UBmales = new List<UBMale>();

        internal static string Vaginal_IK_Name = "Vaginal_IK";
        internal static string Anal_IK_Name = "Anal_IK";
        internal static string Oral_IK_Name = "Oral_IK";


        private void Awake()
        {
            Harmony.CreateAndPatchAll(typeof(Hooks));
        }

        private class Hooks
        {
            [HarmonyPostfix, HarmonyPatch(typeof(Female), nameof(Female.SetHeroineID))]
            private static void AddFemaleUBControl(Female __instance)
            {
                UBFemale UBfemale = __instance.gameObject.AddComponent<UBFemale>();
                UBfemale.Init(__instance);
                UBfemales.Add(UBfemale);
            }

            [HarmonyPostfix, HarmonyPatch(typeof(Male), nameof(Male.SetMaleID))]
            private static void AddMaleUBControl(Male __instance)
            {
                UBMale UBmale = __instance.gameObject.AddComponent<UBMale>();
                UBmale.Init(__instance);
                UBmales.Add(UBmale);
            }

            private static void CleanUBfemales()
            {
                var toRemove = new List<UBFemale>();
                foreach (UBFemale UBfemale in UBfemales)
                {
                    if (UBfemale == null)
                    {
                        toRemove.Add(UBfemale);
                    }
                }
                UBfemales.RemoveAll(script => toRemove.Contains(script));
            }

            private static void CleanUBmales()
            {
                var toRemove = new List<UBMale>();
                foreach (UBMale UBmale in UBmales)
                {
                    if (UBmale == null)
                    {
                        toRemove.Add(UBmale);
                    }
                }
                UBmales.RemoveAll(script => toRemove.Contains(script));
            }

            private static UBFemale FindUBFemale(Transform transform)
            {
                CleanUBfemales();
                foreach (UBFemale UBfemale in UBfemales)
                {
                    if (transform.IsChildOf(UBfemale.transform))
                    {
                        return UBfemale;
                    }
                }
                return null;
            }

            private static UBMale FindUBMale(Transform transform)
            {
                CleanUBmales();
                foreach (UBMale UBmale in UBmales)
                {
                    if (transform.IsChildOf(UBmale.transform))
                    {
                        return UBmale;
                    }
                }
                return null;
            }


            [HarmonyPostfix, HarmonyPatch(typeof(Female), nameof(Female.Apply))]
            private static void SetMainBodyMaterials()
            {
                CleanUBfemales();
                foreach (UBFemale UBfemale in UBfemales)
                {
                    UBfemale.SetMainBodyMaterials();
                }
            }

            [HarmonyPostfix, HarmonyPatch(typeof(Wears), nameof(Wears.WearInstantiate))]
            private static void SetFemaleUncensorBodyofTop(WEAR_TYPE type, Human ___human)
            {
                if (type == WEAR_TYPE.TOP)
                {
                    UBFemale UBfemale = FindUBFemale(___human.transform);
                    if (UBfemale != null) UBfemale.SetTopBody();
                }
            }

            [HarmonyPostfix, HarmonyPatch(typeof(EditMode), nameof(EditMode.ShowUI))]
            private static void PauseTopBodyCaching(bool show)
            {
                UBFemale.PauseCaching = show;
            }

            [HarmonyPostfix, HarmonyPatch(typeof(GameControl), nameof(GameControl.ChangeScene))]
            private static void ResumeTopBodyCaching()
            {
                UBFemale.PauseCaching = false;
            }



            [HarmonyPrefix, HarmonyPatch(typeof(IK_Control), "SetIK")]
            private static void AlternativeIK(IK_Data.PART part, ref Transform target, IK_Control __instance, Transform ___tinRoot)
            {
                if (___tinRoot == null) return;

                if (target.name.Contains("k_f_kokan_00"))
                {
                    UBFemale UBfemale = FindUBFemale(target);
                    UBMale UBmale = FindUBMale(___tinRoot);

                    UBfemale.OpenVagina(part, __instance);
                    UBmale.adjustedFemale = UBfemale;

                    if (part == IK_Data.PART.TIN)
                    {
                        target = UBfemale.Vaginal_IK;
                        UBmale.insertingVagina = true;
                    }
                    else UBmale.pettingVagina = true;
                }

                else if (target.name.Contains("k_f_ana_00"))
                {
                    UBFemale UBfemale = FindUBFemale(target);
                    UBMale UBmale = FindUBMale(___tinRoot);

                    UBmale.adjustedFemale = UBfemale;

                    if (part == IK_Data.PART.TIN)
                    {
                        target = UBfemale.Anal_IK_S;
                    }
                    else UBmale.pettingAna = true;
                }

                else if (target.name.Contains("k_f_head_03"))
                {
                    UBFemale UBfemale = FindUBFemale(target);
                    UBMale UBmale = FindUBMale(___tinRoot);

                    UBmale.adjustedFemale = UBfemale;

                    if (part == IK_Data.PART.TIN)
                    {
                        target = UBfemale.Oral_IK;
                    }
                }
            }

            [HarmonyPrefix, HarmonyPatch(typeof(H_Members), nameof(H_Members.LateUpdate))]
            private static void PreFBIKUpdate()
            {
                foreach (UBMale UBmale in UBmales) if (UBmale.insertingVagina || UBmale.pettingVagina || UBmale.pettingAna) UBmale.PostAdjust();
                foreach (UBFemale UBfemale in UBfemales) if (UBfemale.VaginaItem || UBfemale.AnalItem) UBfemale.PostAdjust();
            }



            [HarmonyPrefix, HarmonyPatch(typeof(H_Item), nameof(H_Item.SetTarget))]
            private static void AlternativeItemIK(ref Transform target, H_Item __instance)
            {
                if (target.name.Contains("k_f_kokan_00"))
                {
                    UBFemale UBfemale = FindUBFemale(target);

                    UBfemale.InsertItem_V = __instance;
                    UBfemale.VaginaItem = true;
                    UBfemale.VaginaOpen = true;
                }

                else if (target.name.Contains("k_f_ana_00"))
                {
                    UBFemale UBfemale = FindUBFemale(target);

                    UBfemale.InsertItem_A = __instance;
                    UBfemale.AnalItem = true;
                }

                else if (target.name.Contains("k_f_head_03")) target = target.Find("Oral_IK");
            }

            [HarmonyPostfix, HarmonyPatch(typeof(H_Members), "ClearIK")]
            private static void ResetIK()
            {
                CleanUBfemales();
                CleanUBmales();
                foreach (UBFemale UBfemale in UBfemales)
                {
                    UBfemale.IKResetUBfemale();
                }
                foreach (UBMale UBmale in UBmales)
                {
                    UBmale.ResetUBmale();
                }
            }
        }
    }
}