
using UdonSharp;
using UdonVR.DisBridge;

public abstract class UserAccessList_Class : UdonSharpBehaviour
{
    public UserAccessList userAccessList;
    public virtual void _ReceivePermissionChange()
    {
        
    }
}
