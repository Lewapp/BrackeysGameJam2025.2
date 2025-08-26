using UnityEngine;

public interface ITargetSeeker 
{
    public Transform target { get; set; }

    public void CanSwitchFocus() { }
}
