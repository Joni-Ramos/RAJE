﻿using Raje.DL.Request.Admin.Base;
using Raje.DL.Response.Base;
using Raje.DL.Services.BLL.Base;
using Raje.DL.Services.DAL.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Raje.API.Controllers.Base
{
    public abstract class RajeBaseCRUDController<TEntity, TResponse, TRequest, TSearchRequest, TSearchResponse> : RajeBaseController
        where TEntity : IEntity
        where TResponse : IBaseResponse
        where TRequest : IBaseRequest
        where TSearchRequest : IBaseSearchRequests
        where TSearchResponse : IBaseResponse
    {
        public readonly ICRUDBusinessService<TEntity, TResponse, TRequest, TSearchRequest, TSearchResponse> _service;

        public RajeBaseCRUDController(ICRUDBusinessService<TEntity, TResponse, TRequest, TSearchRequest, TSearchResponse> service) : base()
        {
            _service = service;
        }

        [HttpGet("{id}")]
        [SwaggerResponse(StatusCodes.Status200OK)]
        [SwaggerResponse(StatusCodes.Status404NotFound)]
        [SwaggerResponse(StatusCodes.Status500InternalServerError)]
        public virtual async Task<ActionResult<TResponse>> Get(long id)
        {
            try
            {
                var TResponse = await _service.GetById(id);
                return Ok(TResponse);
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
        }


        [HttpGet("GetAll")]
        [SwaggerResponse(StatusCodes.Status200OK)]
        [SwaggerResponse(StatusCodes.Status500InternalServerError)]
        public virtual async Task<ActionResult<IEnumerable<TResponse>>> GetAll(bool? active = null)
        {
            var retorno = await _service.GetAll(active);
            if (retorno != null && retorno.Count() > 0)
                return Ok(retorno);
            else
                return Ok(Array.Empty<TResponse>());
        }

        [HttpPost]
        [SwaggerResponse(StatusCodes.Status201Created)]
        [SwaggerResponse(StatusCodes.Status400BadRequest)]
        [SwaggerResponse(StatusCodes.Status401Unauthorized)]
        [SwaggerResponse(StatusCodes.Status500InternalServerError)]
        public virtual async Task<IActionResult> Post([FromBody] TRequest model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    long id = await _service.Create(model);
                    return Created(id.ToString(), null);
                }
                catch (ArgumentNullException ex)
                {
                    return BadRequest(ex.ParamName);
                }
                catch (Exception ex)
                {
                    return BadRequest(ex.Message);
                }
            }

            return BadRequest(ModelState);
        }

        [HttpPut("{id}")]
        [SwaggerResponse(StatusCodes.Status204NoContent)]
        [SwaggerResponse(StatusCodes.Status400BadRequest)]
        [SwaggerResponse(StatusCodes.Status401Unauthorized)]
        [SwaggerResponse(StatusCodes.Status404NotFound)]
        [SwaggerResponse(StatusCodes.Status500InternalServerError)]
        public virtual async Task<IActionResult> Put(long id, [FromBody] TRequest model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    await _service.Update(id, model);
                    return NoContent();
                }
                catch (KeyNotFoundException)
                {
                    return NotFound();
                }
                catch (ArgumentNullException ex)
                {
                    return BadRequest(ex.ParamName);
                }
                catch (Exception ex)
                {
                    if (ex.InnerException == null)
                        return BadRequest(ex.Message);
                    else
                        return BadRequest(ex.InnerException.Message);
                }
            }

            return BadRequest(ModelState);
        }

        [HttpDelete("{id}")]
        [SwaggerResponse(StatusCodes.Status204NoContent)]
        [SwaggerResponse(StatusCodes.Status401Unauthorized)]
        [SwaggerResponse(StatusCodes.Status404NotFound)]
        [SwaggerResponse(StatusCodes.Status500InternalServerError)]
        public virtual IActionResult Delete(int id)
        {
            try
            {
                _service.Delete(id);
                return NoContent();
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("Search")]
        [SwaggerResponse(StatusCodes.Status200OK)]
        [SwaggerResponse(StatusCodes.Status404NotFound)]
        [SwaggerResponse(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<BaseSearchResponse<TSearchResponse>>> Search([FromBody] TSearchRequest search)
        {
            try
            {
                var searchResult = await _service.Search(search);
                return Ok(searchResult);
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
        }
    }
}