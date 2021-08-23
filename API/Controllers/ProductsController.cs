
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using Core.Entities;
using Core.Interfaces;
using Core.Specifications;
using API.DTOs;
using System.Linq;
using AutoMapper;
using Microsoft.Extensions.Configuration;
using API.Helpers;

namespace API.Controllers
{
	public class ProductsController : BaseApiController
	{
		private readonly IGenericRepository<Product> _productsRepo;
		private readonly IGenericRepository<ProductBrand> _productBrandRepo;
		private readonly IGenericRepository<ProductType> _productTypeRepo;
		private readonly IMapper _mapper;
		private readonly IConfiguration _config;

		public ProductsController(
			IGenericRepository<Product> productsRepo,
			IGenericRepository<ProductBrand> productBrandRepo,
			IGenericRepository<ProductType> productTypeRepo,
			IMapper mapper,
			IConfiguration config
		)
		{
			_productsRepo = productsRepo;
			_productBrandRepo = productBrandRepo;
			_productTypeRepo = productTypeRepo;
			_mapper = mapper;
			_config = config;
		}

		[HttpGet]
		public async Task<ActionResult<Pagination<ProductToReturnDto>>> GetProducts([FromQuery] ProductSpecParams productParams)
		{
			var spec = new ProductsWithTypesAndBrandsSpecification(productParams);
			var countSpec = new ProductWithFiltersForCountSpecification(productParams);

			var totalItems = await _productsRepo.CountAsync(countSpec);

			var products = await _productsRepo.ListAsync(spec);

			var data = _mapper
			.Map<IReadOnlyList<Product>, IReadOnlyList<ProductToReturnDto>>(products);

			return Ok(new Pagination<ProductToReturnDto>(productParams.PageIndex, productParams.PageSize, totalItems, data));
		}

		[HttpGet("{id}")]
		public async Task<ActionResult<ProductToReturnDto>> GetProduct(int id)
		{
			var spec = new ProductsWithTypesAndBrandsSpecification(id);

			var product = await _productsRepo.GetEntityWithSpec(spec);

			if (product != null)
			{
				return new ProductToReturnDto
				{
					Id = product.Id,
					Name = product.Name,
					Description = product.Description,
					Price = product.Price,
					PictureUrl = _config["ApiUrl"] + product.PictureUrl,
					ProductType = product.ProductType.Name,
					ProductBrand = product.ProductBrand.Name
				};
			}

			return NotFound("Product with that id was not found");
		}

		[HttpGet("brands")]
		public async Task<ActionResult<IReadOnlyList<ProductBrand>>> GetProductBrands()
		{
			return Ok(await _productBrandRepo.ListAllAsync());
		}

		[HttpGet("types")]
		public async Task<ActionResult<IReadOnlyList<ProductType>>> GetProductTypes()
		{
			return Ok(await _productTypeRepo.ListAllAsync());
		}
	}
}
