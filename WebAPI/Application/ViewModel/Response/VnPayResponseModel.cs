namespace Application.ViewModel.Response
{
    public class VnPayResponseModel
    {
        public string TransactionStatus { get; set; }
        public string OrderId { get; set; }
        public string Message { get; set; }
        public bool IsSuccess { get; set; }
    }
} 