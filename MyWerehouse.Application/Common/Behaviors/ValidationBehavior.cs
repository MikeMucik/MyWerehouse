using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;

namespace MyWerehouse.Application.Common.Behaviors
{
	public class ValidationBehavior<TRequest, TRespone> : IPipelineBehavior<TRequest, TRespone>
		where TRequest : IRequest<TRespone>
	{
		private readonly IEnumerable<IValidator<TRequest>> _validators;
		public ValidationBehavior(IEnumerable<IValidator<TRequest>> validators)
		{
			_validators = validators;
		}
		public async Task<TRespone> Handle(TRequest request, RequestHandlerDelegate<TRespone> next, 
			CancellationToken cancellationToken)
		{
			if(_validators.Any())
			{
				var context = new ValidationContext<TRequest>(request);

				var validatonResults = await Task.WhenAll(
					_validators.Select(v => v.ValidateAsync(context, cancellationToken)));

				var failure = validatonResults
					.SelectMany(r=>r.Errors)
					.Where(f=>f != null)
					.ToList();

				if (failure.Any())
				{
					throw new ValidationException(failure);
				}
			}
			return await next();
		}
	}
}
