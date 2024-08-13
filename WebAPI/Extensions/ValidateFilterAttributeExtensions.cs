//using Microsoft.AspNetCore.Mvc.Filters;
//using System.Text;
//using WebAPI.Exceptions;

//namespace WebAPI.Extensions
//{
//    public static class ValidateFilterAttributeExtensions
//    {
//        public static void ValidateModelState(this ActionExecutingContext context)
//        {
//            if (!context.ModelState.IsValid)
//            {
//                var sb = new StringBuilder(100);

//                foreach (var item in context.ModelState)
//                {
//                    foreach (var error in item.Value.Errors)
//                        sb.AppendLine(error.ErrorMessage);
//                }

//                throw new BadRequestException(sb.ToString());
//            }
//        }
//    }
//}
