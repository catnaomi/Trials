using System;

[Serializable]
public struct AllScenesSaveData {
    [Serializable]
    public struct Dojo {
        public bool isIceGiantDead;
        public bool isShipHingeDown;
        public bool isGolemEncounterComplete;
    };

    public Dojo dojo;
}
