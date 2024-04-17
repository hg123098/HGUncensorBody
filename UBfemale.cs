using IllusionUtility.GetUtility;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.Rendering;

namespace UncensorBody
{
    public class UBFemale : MonoBehaviour
    {
        private Female female;
        private Transform BaseBoneRoot;
        private GameObject OrgMainBody;
        private SkinnedMeshRenderer bodyskinMesh;
        private GameObject wearsRoot;

        private GameObject uncensorbody;
        private GameObject UpperBodyMain;
        private GameObject LowerBodyPlain;
        private GameObject LowerBodyVagina;
        private bool MainbodyPrepared;
        

        private static Dictionary<int, Mesh> CachedTopBodyMesh_A = new Dictionary<int, Mesh>();
        private static Dictionary<int, Mesh> CachedTopBodyMesh_B = new Dictionary<int, Mesh>();
        internal static bool PauseCaching;
        internal bool UseOrgTopBody;


        internal static bool UseOrgBodyCollider;
        private static bool DisplayBodyCollider;
        private List<GameObject> OrgBodyColliders = new List<GameObject>();
        private List<GameObject> NewBodyColliders = new List<GameObject>();


        private IK_Control InsertMale;
        private bool WaitTin;

        internal bool VaginaOpen;

        internal H_Item InsertItem_V;
        internal bool VaginaItem;
        internal H_Item InsertItem_A;
        internal bool AnalItem;

        internal Transform cf_J_Kosi02_s;
        internal Transform cf_J_Kokan;
        internal Transform cf_J_Ana;
        internal Transform cf_J_Head_s;
        internal Transform k_f_kokan_00;
        internal Transform k_f_ana_00;
        internal Transform k_f_head_03;


        internal Transform Vaginal_IK;
        internal Transform Anal_IK_S;
        internal Transform Anal_IK_A;
        internal Transform Oral_IK;


        public void Init(Female FemaleScript)
        {
            female = FemaleScript;
            BaseBoneRoot = this.transform.Find("p_cf_anim/cf_J_Root");
            OrgMainBody = Transform_Utility.FindTransform(this.transform, "cf_O_body_00").gameObject;
            bodyskinMesh = OrgMainBody.GetComponent<SkinnedMeshRenderer>();
            wearsRoot = this.transform.Find("Wears").gameObject;

            cf_J_Kosi02_s = Transform_Utility.FindTransform(BaseBoneRoot, "cf_J_Kosi02_s");
            cf_J_Kokan = Transform_Utility.FindTransform(cf_J_Kosi02_s, "cf_J_Kokan");
            cf_J_Ana = Transform_Utility.FindTransform(cf_J_Kosi02_s, "cf_J_Ana");
            cf_J_Head_s = Transform_Utility.FindTransform(BaseBoneRoot, "cf_J_Head_s");
            k_f_kokan_00 = Transform_Utility.FindTransform(cf_J_Kokan, "k_f_kokan_00");
            k_f_ana_00 = Transform_Utility.FindTransform(cf_J_Ana, "k_f_ana_00");
            k_f_head_03 = Transform_Utility.FindTransform(cf_J_Head_s, "k_f_head_03");


            SetMainBodyMesh();
            SetMainBodyCollider();
            SetMainBodyTransform();
        }

        private void SetMainBodyMesh()
        {
            uncensorbody = AssetBundleLoader.LoadAndInstantiate<GameObject>(UncensorBody.abDataPath, UncensorBody.abName, "HGUncensorBody");
            if (uncensorbody == null) return;
            uncensorbody.transform.SetParent(wearsRoot.transform, false);

            Transform transform = Transform_Utility.FindTransform(uncensorbody.transform, "cf_N_O_root");
            if (transform != null)
            {
                AttachBoneWeight.Attach(BaseBoneRoot.gameObject, transform.gameObject, true);
            }
            SkinnedMeshRenderer[] componentsInChildren = uncensorbody.GetComponentsInChildren<SkinnedMeshRenderer>(true);
            foreach (Renderer renderer in componentsInChildren)
            {
                if (renderer.name == "UncensorBody_Upper_Main") UpperBodyMain = renderer.gameObject;
                else if (renderer.name == "UncensorBody_Lower_Plain") LowerBodyPlain = renderer.gameObject;
                else if (renderer.name == "UncensorBody_Lower_Vagina") LowerBodyVagina = renderer.gameObject;
            }
        }

        private void SetMainBodyCollider()
        {
            foreach(MeshCollider collider in BaseBoneRoot.GetComponentsInChildren<MeshCollider>())
            {
                if (collider.name.Contains("hit_") && !collider.name.Contains("sakura_head")) OrgBodyColliders.Add(collider.gameObject);
            }

            foreach (MeshCollider collider in uncensorbody.GetComponentsInChildren<MeshCollider>())
            {
                if (collider.name.Contains("Coll.")) NewBodyColliders.Add(collider.gameObject);
            }

            Transform[] transforms = BaseBoneRoot.GetComponentsInChildren<Transform>();
            foreach(GameObject newCollider in NewBodyColliders)
            {
                foreach (Transform transform in transforms)
                {
                    if( transform.name == newCollider.name.Substring(5))
                    {
                        newCollider.transform.SetParent(transform, false);
                        break;
                    }
                }
            }

            foreach (GameObject orgCollider in OrgBodyColliders)
            {
                orgCollider.SetActive(false);
            }
        }

        public void ChangeBodyCollider()
        {
            UseOrgBodyCollider = !UseOrgBodyCollider;
            foreach (GameObject orgCollider in OrgBodyColliders)
            {
                orgCollider.SetActive(UseOrgBodyCollider);
            }

            foreach (GameObject newCollider in NewBodyColliders)
            {
                newCollider.SetActive(!UseOrgBodyCollider);
            }
        }
        
        internal void SetMainBodyMaterials()
        {
            if(UpperBodyMain != null && LowerBodyPlain != null && LowerBodyVagina != null)
            {
                foreach (GameObject MainBody in new GameObject[]{UpperBodyMain, LowerBodyPlain, LowerBodyVagina})
                {
                    MainBody.GetComponent<SkinnedMeshRenderer>().sharedMaterials = bodyskinMesh.sharedMaterials;
                    MainBody.GetComponent<SkinnedMeshRenderer>().reflectionProbeUsage = ReflectionProbeUsage.Off;
                }

                MainbodyPrepared = true;
            }
        }

        public void SetTopBody()
        {
            UseOrgTopBody = true;

            WearData TopWearData = female.wears.GetWearData(Character.WEAR_TYPE.TOP);
            WearObj TopWearObj = female.wears.GetWearObj(Character.WEAR_TYPE.TOP);
            if (TopWearObj == null) return;

            GameObject TopRoot = TopWearObj.obj;
            SkinnedMeshRenderer TopBody_A = FindTopBodyMeshObject(TopRoot, "N_top_a");
            SkinnedMeshRenderer TopBody_B = FindTopBodyMeshObject(TopRoot, "N_top_b");

            if(TopBody_A != null)
            {
                if (!CachedTopBodyMesh_A.ContainsKey(TopWearData.id))
                {
                    if (PauseCaching)
                    {
                        UseOrgTopBody = true;
                        return;
                    }
                    Mesh mesh = MakeTopBodyMesh(TopRoot, TopBody_A);
                    CachedTopBodyMesh_A.Add(TopWearData.id, mesh);
                }
                UseOrgTopBody = false;
                TopBody_A.sharedMesh = CachedTopBodyMesh_A[TopWearData.id];
            }
            if (TopBody_B != null)
            {
                if (!CachedTopBodyMesh_B.ContainsKey(TopWearData.id))
                {
                    if (PauseCaching)
                    {
                        UseOrgTopBody = true;
                        return;
                    }
                    Mesh mesh = MakeTopBodyMesh(TopRoot, TopBody_B);
                    CachedTopBodyMesh_B.Add(TopWearData.id, mesh);
                }
                UseOrgTopBody = false;
                TopBody_B.sharedMesh = CachedTopBodyMesh_B[TopWearData.id];
            }
        }

        private SkinnedMeshRenderer FindTopBodyMeshObject(GameObject TopRoot, string name)
        {
            Transform TopMesh = Transform_Utility.FindTransform(TopRoot.transform, name);
            if (TopMesh != null)
            {
                Transform HS1_unc = Transform_Utility.FindTransform_Partial(TopMesh, "cf_O_unc_");
                if (HS1_unc != null && !PauseCaching) Destroy(HS1_unc.gameObject);

                return Transform_Utility.FindTransform_Partial(TopMesh, "body").GetComponentInChildren<SkinnedMeshRenderer>();
            }
            return null;
        }

        private Mesh MakeTopBodyMesh(GameObject TopRoot, SkinnedMeshRenderer OrgTopBody)
        {
            GameObject quadMesh = new GameObject();
            MeshFilter meshFilter = quadMesh.AddComponent<MeshFilter>();
            quadMesh.AddComponent<MeshRenderer>();
            Mesh mesh = new Mesh();
            mesh.vertices = new Vector3[]
            {
                new Vector3(-1,0,1), new Vector3(1,0,1), new Vector3(-1,0,-1), new Vector3(1,0,-1)
            };
            mesh.uv = new Vector2[]
            {
                new Vector2(0.0f,1.0f), new Vector2(1.0f,1.0f), new Vector2(0.0f,0.0f), new Vector2(1.0f,0.0f)
            };
            mesh.triangles = new int[] { 0, 1, 2, 2, 1, 3 };
            mesh.RecalculateNormals();
            meshFilter.mesh = mesh;

            quadMesh.transform.SetParent(Transform_Utility.FindTransform(TopRoot.transform, "cf_J_Kosi01"), false);
            quadMesh.transform.localRotation = Quaternion.Euler(180, 0, 0);

            Mesh newBodyMesh = MeshSplitter.splitByQuad(OrgTopBody, meshFilter);
            newBodyMesh.name = "UncensorBody_Upper_Top";
            Destroy(quadMesh);
            return newBodyMesh;
        }

        private void Update()
        {
            if (UncensorBody._sNewFemaleBodyCollider.Value != !UseOrgBodyCollider) ChangeBodyCollider();

            if (WaitTin) VaginaOpen = InsertMale.TinEnable;

            if (VaginaItem) if (InsertItem_V == null) VaginaItem = false;
            if (AnalItem) if (InsertItem_A == null) AnalItem = false;

            if (MainbodyPrepared)
            {
                UpperBodyMain.SetActive(OrgMainBody.activeInHierarchy);
                bodyskinMesh.enabled = false;

                LowerBodyPlain.SetActive(!VaginaOpen);
                LowerBodyVagina.SetActive(VaginaOpen);
            }

            if (UseOrgTopBody)
            {
                if(!(UpperBodyMain.activeInHierarchy))
                {
                    LowerBodyPlain.SetActive(false);
                    LowerBodyVagina.SetActive(false);
                }
            }
        }

        public void OpenVagina(IK_Data.PART part, IK_Control insertHuman)
        {
            if (part == IK_Data.PART.TIN)
            {
                InsertMale = insertHuman;
                WaitTin = true;
            }
            else VaginaOpen = true;
        }

        public void IKResetUBfemale()
        {
            InsertMale = null;
            WaitTin = false;
            if (!VaginaItem) VaginaOpen = false;
        }


        private void SetMainBodyTransform()
        {
            Vaginal_IK = new GameObject(UncensorBody.Vaginal_IK_Name).transform;
            Vaginal_IK.gameObject.layer = 8;
            Vaginal_IK.SetParent(cf_J_Kosi02_s, false);
            Vaginal_IK.localPosition = new Vector3(0f, -0.07f, -0.01f);

            Anal_IK_S = new GameObject(UncensorBody.Anal_IK_Name + "_S").transform;
            Anal_IK_S.gameObject.layer = 8;
            Anal_IK_S.SetParent(cf_J_Ana, false);
            Anal_IK_S.localPosition = new Vector3(0f, 0.02f, 0f);
            Anal_IK_A = new GameObject(UncensorBody.Anal_IK_Name + "_A").transform;
            Anal_IK_A.gameObject.layer = 8;
            Anal_IK_A.SetParent(cf_J_Ana, false);
            Anal_IK_A.localPosition = new Vector3(0f, 0f, 0.01f);

            Oral_IK = new GameObject(UncensorBody.Oral_IK_Name).transform;
            Oral_IK.gameObject.layer = 8;
            Oral_IK.SetParent(k_f_head_03, false);
            Oral_IK.localPosition = new Vector3(0f, 0f, -0.02f);
        }

        internal void PostAdjust()
        {
            Vector3 Vdelta = Vaginal_IK.position - cf_J_Kokan.position;
            Vector3 Adelta = Anal_IK_A.position - cf_J_Ana.position;

            if (VaginaItem) k_f_kokan_00.position += Vdelta;
            if (AnalItem) k_f_ana_00.position += Adelta;
        }

        public void ShowFemale(bool show)
        {
            uncensorbody.SetActive(show);
        }

    }
}

