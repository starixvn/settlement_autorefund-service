using System.Text.Json;

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

    public bool MarkFail { get; set; } = true;


    public override string ToString() => JsonSerializer.Serialize(this);
}