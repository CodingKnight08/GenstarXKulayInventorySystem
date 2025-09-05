using static GenstarXKulayInventorySystem.Shared.Helpers.ProductsEnumHelpers;

namespace GenstarXKulayInventorySystem.Client;

public class UserState
{
    public BranchOption? Branch { get; private set; }

    public void SetBranch(BranchOption? branch)
    {
        Branch = branch;
    }
}
