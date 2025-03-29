using UnityEngine;
using UnityEngine.UIElements;

using UnityEngine.UIElements;

namespace Script.Quests
{
    public class NPCIconController : MonoBehaviour
    {
        public Sprite Icon;
        private GameObject NPC_icon;

        public void ChangeIcon()
        {
            NPC_icon = GameObject.Find("icon");
            NPC_icon.GetComponent<UnityEngine.UI.Image>().sprite = Icon;
        }
    }
}