using UnityEngine;

namespace UncensorBody
{
    public class UBMale : MonoBehaviour
    {
        private Male male;
        private Transform cm_J_dan100_00;

        internal bool insertingVagina;
        internal bool pettingVagina;
        internal bool pettingAna;
        internal UBFemale adjustedFemale;

        public void Init(Male MaleScript)
        {
            male = MaleScript;
            cm_J_dan100_00 = Transform_Utility.FindTransform(this.transform, "cm_J_dan100_00");
        }

        internal void ResetUBmale()
        {
            insertingVagina = false;
            pettingVagina = false;
            pettingAna = false;
            adjustedFemale = null;
        }

        internal void PostAdjust()
        {
            Vector3 Vdelta = adjustedFemale.Vaginal_IK.position - adjustedFemale.cf_J_Kokan.position;
            Vector3 Adelta = adjustedFemale.Anal_IK_A.position - adjustedFemale.cf_J_Ana.position;

            if (insertingVagina) cm_J_dan100_00.position += Vdelta;
            if (pettingVagina) adjustedFemale.k_f_kokan_00.position += Vdelta;
            if (pettingAna) adjustedFemale.k_f_ana_00.position += Adelta;
        }

    }
}
