using UnityEngine;

public interface IClickHandler {
    void Click(GameObject clickObject);
    void FocusEnter(GameObject focusEnterObject);
    void FocusExit(GameObject focusExitObject);
}
