﻿using System.Collections.Generic;
using Locima.SlidingBlock.GameTemplates;

namespace Locima.SlidingBlock.IO
{

    /// <summary>
    /// An interface for managing game templates on the app
    /// </summary>
    public interface IGameTemplateManager
    {
        /// <summary>
        /// Initialise the template manager, this is invoked once during each app start-up.
        /// </summary>
        void Initialise();

        /// <summary>
        /// Gets a list of all the game templates, allowing the caller to specify whether shadows should be included and, if so, whether objects which have shadows
        /// should be omitted from the returned list
        /// </summary>
        /// <param name="includeShadows">If <c>true</c>, then shadow game template will be returned (i.e. ones which are mid-edit)</param>
        /// <param name="collapseShadows">If <c>true</c>, then objects which have shadows will be omitted from the list (only valid if <paramref name="includeShadows"/> is also true</param>
        /// <returns>A list of game template, never null</returns>
        List<GameTemplate> GetGameTemplates(bool includeShadows, bool collapseShadows);

        /// <summary>
        /// Loads the game template specified by its it in <paramref name="id"/>
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        GameTemplate Load(string id);

        /// <summary>
        /// Saves the game template passed
        /// </summary>
        /// <param name="gameTemplate"></param>
        void Save(GameTemplate gameTemplate);

        /// <summary>
        /// Deletes the game template specified by its ID in <paramref name="id"/>
        /// </summary>
        /// <param name="id">The ID of the <see cref="GameTemplate"/> to remove</param>
        void Delete(string id);

        /// <summary>
        /// Overwrites the object that this object is shadowing with this shadow
        /// </summary>
        /// <param name="shadow">The shadow template to promote</param>
        /// <returns>The ID of the object this has been promoted to</returns>
        string PromoteShadow(GameTemplate shadow);

    }
}