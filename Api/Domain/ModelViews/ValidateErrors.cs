namespace MinimalApi.Domain.ModelViews;

public struct ValidateErrors {
    public ValidateErrors()
    {
    }

    public List<string> Messages { get; set;} = [];
}