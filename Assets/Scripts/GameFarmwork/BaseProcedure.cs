using System.Threading.Tasks;

/// <summary>
/// 作者: Teddy
/// 时间: 2018/03/01
/// 功能: 
/// </summary>
public abstract class BaseProcedure
{
    public async Task ChangeProcedure<T>(object value = null) where T : BaseProcedure
    {
        await GameManager.Procedure.ChangeProcedure<T>(value);
    }

    public virtual async Task OnEnterProcedure(object value)
    {
        await Task.Yield();
    }

    public virtual async Task OnLeaveProcedure()
    {
        await Task.Yield();
    }
}
