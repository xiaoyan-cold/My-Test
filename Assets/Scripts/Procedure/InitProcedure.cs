using System.Threading.Tasks;


public class InitProcedure : BaseProcedure
{
    public override async Task OnEnterProcedure(object value)
    {
        UnityLog.Info("enter init procedure");
        await Task.Yield();



    }
}

