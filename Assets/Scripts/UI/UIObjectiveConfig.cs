using System;
using System.Collections.Generic;
using UnityEngine;

public class UIObjectiveConfig : MonoBehaviour
{
    [Serializable]
    public class UIObjectiveInfo
    {
        public Objective Objective;

        public Sprite Icon;

        public string HintIcon;
    }

    public List<UIObjectiveInfo> ObjectiveInfos;

    public UIObjectiveInfo GetObjectiveInfo(Type type)
    {
        return ObjectiveInfos.Find((UIObjectiveInfo o) => o.Objective.GetType() == type);
    }
}
