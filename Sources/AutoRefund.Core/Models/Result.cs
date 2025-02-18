namespace AutoRefund.Core.Models;

public class Result
{
    public Result()
    {
        ErrCode = 0;
        ErrMsg = "";
    }

    public int ErrCode { get; set; }

    public string ErrMsg { get; set; }

    public bool IsSuccess => ErrCode == 0;
}