﻿using obras_1213.Models;
using obras_1213.Models.View;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace obras_1213.Controllers
{
    public class ObrasController : Controller
    {
        //
        // GET: /Obras/
        [HttpGet, Authorize]
        public ActionResult Index()
        {
            return View(Work.FindAll());
        }

        [HttpGet, Authorize]
        public ActionResult Details(int id)
        {
            try
            {
                return View("Details", new WorkViewModel(Work.Find(id)));
            }
            catch (ModelException ex)
            {
                ModelState.AddModelError("", ex);
                return View("Index");
            }
        }

        [HttpGet, Authorize(Roles = "receptionist")]
        public ActionResult New()
        {
            return View(new NewWorkViewModel());
        }

        [HttpPost, Authorize(Roles = "receptionist")]
        public ActionResult New(NewWorkViewModel newWork)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    return RedirectToAction("Details", new { id = Work.Insert(1, newWork.CarLicensePlate, newWork.Actions) });
                }
                catch (ModelException ex)
                {
                    ModelState.AddModelError("", ex.Message);
                }
            }
            return View(newWork);
        }

        [Authorize(Roles = "receptionist")]
        public ActionResult ChangeState(int id, string state)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    Work.Find(id).State = state;
                    return RedirectToAction("Details", new { id = id });
                }
                catch (ModelException ex)
                {
                    ModelState.AddModelError("", ex.Message);
                }
            }
            return Details(id);
        }

        [HttpPost, Authorize(Roles = "receptionist")]
        public ActionResult AddAction(int id, int actionId, int employeeId)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    if (Work.Find(id).AddAction(actionId, employeeId))
                    {
                        return RedirectToAction("Details", new { id = id });
                    }
                    ModelState.AddModelError("", "Não foi possível adicionar o acto à obra.");
                }
                catch (ModelException ex)
                {
                    ModelState.AddModelError("", ex.Message);
                }
            }
            return Details(id);
        }

        [Authorize(Roles = "receptionist")]
        public ActionResult RemoveAction(int id, int actionId)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    if (Work.Find(id).Actions.First(a => a.ID == actionId).Remove())
                    {
                        return RedirectToAction("Details", new { id = id });
                    }
                    ModelState.AddModelError("", "Não foi possível adicionar o acto à obra.");
                }
                catch (ModelException ex)
                {
                    ModelState.AddModelError("", ex.Message);
                }
            }
            return Details(id);
        }

        [Authorize]
        public ActionResult CompleteAction(int id, int actionId)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    WorkAction wa = Work.Find(id).Actions.First(a => a.ID == actionId);
                    if (wa.CurrentUserCanComplete)
                    {
                        wa.Completed = true;
                        return RedirectToAction("Details", new { id = id });
                    }
                    else
                    {
                        ModelState.AddModelError("", "Não tem autorização para completar este acto.");
                    }
                }
                catch (ModelException ex)
                {
                    ModelState.AddModelError("", ex.Message);
                }
            }
            return Details(id);
        }

        [HttpPost, Authorize]
        public ActionResult ChangeActionTime(int id, int actionId, string time)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    float actualTime;
                    if (time != null && float.TryParse(time, out actualTime))
                    {
                        Work.Find(id).Actions.First(a => a.ID == actionId).TimeWorked = actualTime;
                        return RedirectToAction("Details", new { id = id });
                    }
                    ModelState.AddModelError("", "O número de horas não está num formato correcto.");
                }
                catch (ModelException ex)
                {
                    ModelState.AddModelError("", ex.Message);
                }
            }
            return Details(id);
        }

        [HttpPost, Authorize(Roles = "receptionist")]
        public ActionResult RemovePart(int id, string partId)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    if (Work.Find(id).Parts.First(p => p.ID.Equals(partId)).Remove())
                    {
                        return RedirectToAction("Details", new { id = id });
                    }
                    ModelState.AddModelError("", "Não foi possível remover essa peça da obra.");
                }
                catch (ModelException ex)
                {
                    ModelState.AddModelError("", ex.Message);
                }
            }
            return Details(id);
        }

        [HttpPost, Authorize(Roles = "receptionist")]
        public ActionResult AddPart(int id, WorkPart newPart)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    if (Work.Find(id).AddPart(newPart))
                    {
                        return RedirectToAction("Details", new { id = id });
                    }
                    ModelState.AddModelError("", "Não é possível adicionar essa peça à obra. Será que já existe?");
                }
                catch (ModelException)
                {
                    ModelState.AddModelError("", "Não é possível adicionar essa peça à obra. Será que já existe?");
                }
            }
            return Details(id);
        }

        [HttpPost, Authorize(Roles = "receptionist")]
        public ActionResult InvoiceTo(int id, int customerId)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    if (Work.Find(id).Facturar(customerId))
                    {
                        return RedirectToAction("Details", new { id = id });
                    }
                    ModelState.AddModelError("", "Não é possível facturar esta obra?");
                }
                catch (ModelException ex)
                {
                    ModelState.AddModelError("", ex.Message);
                }
            }
            return Details(id);
        }

        [Authorize(Roles = "receptionist")]
        public ActionResult Pay(int id)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    Invoice inv = Work.Find(id).Invoice;
                    if (inv != null)
                    {
                        inv.Paid = true;
                        if (inv.Paid)
                        {
                            return RedirectToAction("Details", new { id = id });
                        }
                    }
                    ModelState.AddModelError("", "Não é possível pagar esta obra?");
                }
                catch (ModelException ex)
                {
                    ModelState.AddModelError("", ex.Message);
                }
            }
            return Details(id);
        }
    }
}
