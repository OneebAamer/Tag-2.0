using UnityEngine;
using Unity.Netcode;
using System.Collections;
using Unity.Collections;
using Unity.Netcode.Components;
using TMPro;

public class GameTimeManager : NetworkBehaviour
{

    public IEnumerator CountdownRoutine(int maxTimerValue, NetworkVariable<int> timer, System.Action onComplete)
    {
        timer.Value = maxTimerValue;
        while (timer.Value > 0)
        {
            yield return new WaitForSeconds(1f);
            timer.Value--;
        }
        onComplete?.Invoke();
    }
}
