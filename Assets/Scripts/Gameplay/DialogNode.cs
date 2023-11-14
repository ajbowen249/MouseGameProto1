using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DialogOption
{
    public string Text;
    public string Tag;
    public DialogNode Node;
}

public class DialogNode
{
    public string Message;
    public List<DialogOption> Options;
}
