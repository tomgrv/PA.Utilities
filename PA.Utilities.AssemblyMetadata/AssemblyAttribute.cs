﻿
using System;
using System.Collections;
using System.Collections.Generic;

namespace PA.Utilities.AssemblyMetadata
{
	[AttributeUsage(AttributeTargets.Assembly)]
	public class AssemblyAuthorsAttribute: Attribute
	{
		public System.Collections.Generic.IEnumerable<System.String> Authors { get; private set; }
		public AssemblyAuthorsAttribute() : this(default(System.Collections.Generic.IEnumerable<System.String>)) { }
		public AssemblyAuthorsAttribute(System.Collections.Generic.IEnumerable<System.String> value) { this.Authors = value; }
		public AssemblyAuthorsAttribute(params System.String[] value) { this.Authors = value; }
	}

	[AttributeUsage(AttributeTargets.Assembly)]
	public class AssemblyOwnersAttribute: Attribute
	{
		public System.Collections.Generic.IEnumerable<System.String> Owners { get; private set; }
		public AssemblyOwnersAttribute() : this(default(System.Collections.Generic.IEnumerable<System.String>)) { }
		public AssemblyOwnersAttribute(System.Collections.Generic.IEnumerable<System.String> value) { this.Owners = value; }
		public AssemblyOwnersAttribute(params System.String[] value) { this.Owners = value; }
	}

	[AttributeUsage(AttributeTargets.Assembly)]
	public class AssemblyIconUrlAttribute: Attribute
	{
		public System.Uri IconUrl { get; private set; }
		public AssemblyIconUrlAttribute() : this(default(System.Uri)) { }
		public AssemblyIconUrlAttribute(System.Uri value) { this.IconUrl = value; }
	}

	[AttributeUsage(AttributeTargets.Assembly)]
	public class AssemblyLicenseUrlAttribute: Attribute
	{
		public System.Uri LicenseUrl { get; private set; }
		public AssemblyLicenseUrlAttribute() : this(default(System.Uri)) { }
		public AssemblyLicenseUrlAttribute(System.Uri value) { this.LicenseUrl = value; }
	}

	[AttributeUsage(AttributeTargets.Assembly)]
	public class AssemblyProjectUrlAttribute: Attribute
	{
		public System.Uri ProjectUrl { get; private set; }
		public AssemblyProjectUrlAttribute() : this(default(System.Uri)) { }
		public AssemblyProjectUrlAttribute(System.Uri value) { this.ProjectUrl = value; }
	}

	[AttributeUsage(AttributeTargets.Assembly)]
	public class AssemblyRequireLicenseAcceptanceAttribute: Attribute
	{
		public System.Boolean RequireLicenseAcceptance { get; private set; }
		public AssemblyRequireLicenseAcceptanceAttribute() : this(default(System.Boolean)) { }
		public AssemblyRequireLicenseAcceptanceAttribute(System.Boolean value) { this.RequireLicenseAcceptance = value; }
	}

	[AttributeUsage(AttributeTargets.Assembly)]
	public class AssemblyDevelopmentDependencyAttribute: Attribute
	{
		public System.Boolean DevelopmentDependency { get; private set; }
		public AssemblyDevelopmentDependencyAttribute() : this(default(System.Boolean)) { }
		public AssemblyDevelopmentDependencyAttribute(System.Boolean value) { this.DevelopmentDependency = value; }
	}

	[AttributeUsage(AttributeTargets.Assembly)]
	public class AssemblySummaryAttribute: Attribute
	{
		public System.String Summary { get; private set; }
		public AssemblySummaryAttribute() : this(default(System.String)) { }
		public AssemblySummaryAttribute(System.String value) { this.Summary = value; }
	}

	[AttributeUsage(AttributeTargets.Assembly)]
	public class AssemblyReleaseNotesAttribute: Attribute
	{
		public System.String ReleaseNotes { get; private set; }
		public AssemblyReleaseNotesAttribute() : this(default(System.String)) { }
		public AssemblyReleaseNotesAttribute(System.String value) { this.ReleaseNotes = value; }
	}

	[AttributeUsage(AttributeTargets.Assembly)]
	public class AssemblyLanguageAttribute: Attribute
	{
		public System.String Language { get; private set; }
		public AssemblyLanguageAttribute() : this(default(System.String)) { }
		public AssemblyLanguageAttribute(System.String value) { this.Language = value; }
	}

	[AttributeUsage(AttributeTargets.Assembly)]
	public class AssemblyTagsAttribute: Attribute
	{
		public System.String Tags { get; private set; }
		public AssemblyTagsAttribute() : this(default(System.String)) { }
		public AssemblyTagsAttribute(System.String value) { this.Tags = value; }
	}

	[AttributeUsage(AttributeTargets.Assembly)]
	public class AssemblyFrameworkAssembliesAttribute: Attribute
	{
		public System.Collections.Generic.IEnumerable<NuGet.FrameworkAssemblyReference> FrameworkAssemblies { get; private set; }
		public AssemblyFrameworkAssembliesAttribute() : this(default(System.Collections.Generic.IEnumerable<NuGet.FrameworkAssemblyReference>)) { }
		public AssemblyFrameworkAssembliesAttribute(System.Collections.Generic.IEnumerable<NuGet.FrameworkAssemblyReference> value) { this.FrameworkAssemblies = value; }
		public AssemblyFrameworkAssembliesAttribute(params NuGet.FrameworkAssemblyReference[] value) { this.FrameworkAssemblies = value; }
	}

	[AttributeUsage(AttributeTargets.Assembly)]
	public class AssemblyPackageAssemblyReferencesAttribute: Attribute
	{
		public System.Collections.Generic.ICollection<NuGet.PackageReferenceSet> PackageAssemblyReferences { get; private set; }
		public AssemblyPackageAssemblyReferencesAttribute() : this(default(System.Collections.Generic.ICollection<NuGet.PackageReferenceSet>)) { }
		public AssemblyPackageAssemblyReferencesAttribute(System.Collections.Generic.ICollection<NuGet.PackageReferenceSet> value) { this.PackageAssemblyReferences = value; }
	}

	[AttributeUsage(AttributeTargets.Assembly)]
	public class AssemblyDependencySetsAttribute: Attribute
	{
		public System.Collections.Generic.IEnumerable<NuGet.PackageDependencySet> DependencySets { get; private set; }
		public AssemblyDependencySetsAttribute() : this(default(System.Collections.Generic.IEnumerable<NuGet.PackageDependencySet>)) { }
		public AssemblyDependencySetsAttribute(System.Collections.Generic.IEnumerable<NuGet.PackageDependencySet> value) { this.DependencySets = value; }
		public AssemblyDependencySetsAttribute(params NuGet.PackageDependencySet[] value) { this.DependencySets = value; }
	}

	[AttributeUsage(AttributeTargets.Assembly)]
	public class AssemblyMinClientVersionAttribute: Attribute
	{
		public System.Version MinClientVersion { get; private set; }
		public AssemblyMinClientVersionAttribute() : this(default(System.Version)) { }
		public AssemblyMinClientVersionAttribute(System.Version value) { this.MinClientVersion = value; }
	}

  
}
