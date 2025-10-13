# Arbor Development Roadmap

## 1. Enhanced Exporters & Output Handling

- **Status:** Complete!
- **Goal:** Expand `tree` output beyond the in-memory Spectre render.
- **Scope:** Add `--export` improvements (streaming JSON/Markdown), introduce `--output <path>` to write directly to
  disk.
- **Rationale:** Unlocks immediate automation/documentation workflows and exercises the functional tree model without
  major refactors.
- **Dependencies:** Existing `TreeExporterFactory`, domain model with relative paths/metadata.

## 2. Advanced Filtering & Branch Pruning

- **Status:** Complete!
- **Goal:** Let users focus on just the aspects of the tree they need.
- **Scope:** Extend include/exclude patterns to directories, add `--include-dir`/`--exclude-dir`, and hide branches left
  empty after filtering.
- **Rationale:** Builds on current filtering logic, dramatically improves usability on larger folder structures.
- **Dependencies:** Filter normalization utilities in `TreeOptions`.

## 3. Rich Metadata Extensions

- **Status:** Complete!
- **Goal:** Show more information about the folders and files, integrated with the tree itself.
- **Scope:** Optional flags for permissions, owners/groups, NTFS attributes... Display via label strategies / exporters.
- **Rationale:** Minimal new plumbing (metadata type already exists) with high value.
- **Dependencies:** Platform-specific metadata collection helpers, existing metadata renderers.

## 4. Symlink Awareness (`--follow-symlinks`)

- **Status:** Not started
- **Goal:** Traverse or annotate symbolic links safely.
- **Scope:** Detect links via `IFileSystem`, add cycle guard recursion, optionally render link targets and depth hints.
- **Rationale:** Useful for real-world trees in my own work, and prevents infinite recursion.
- **Dependencies:** Cached node tracking (maybe?) and expanded warning system.

## 5. Interactive Selection Mode

- **Status:** Completed!
- **Goal:** Provide a keyboard-driven way to explore and cherry-pick nodes.
- **Scope:** Add `arbor interactive` command that loads the tree once, lets the user navigate directories, toggle
  selections, and export the chosen subset to any format (text/Markdown/JSON). Bonus: output equivalent filter command
  for reuse.
- **Rationale:** Makes Arbor approachable for documentation workflows and teaches filters by example, plus complements
  advanced filtering. This would be helpful for quick usage without needing to figure out all patterns to include or exclude!
- **Dependencies:** Completed exporter improvements (#1) and enhanced filtering (#2) so selections can be re-run
  non-interactively.

## 6. Alternate Visualization Modes

- **Status:** Not started
- **Goal:** Offer views tailored to different audiences.
- **Scope:** Implement compact collapsed view, tabular/column view, and sectioned output for broad trees.
- **Rationale:** Leverages exporter pipeline; improves readability without changing domain logic.
- **Dependencies:** Exporters, label strategies; potentially configuration overrides.

## 7. Color & Label Theming

- **Status:** Not started
- **Goal:** Make tree aesthetics configurable.
- **Scope:** Palette per file extension/type, optional `.arborrc`/`.editorconfig` overrides, user-defined badges.
- **Rationale:** Helps teams align Arbor’s output with project conventions; builds on renderer strategy.
- **Dependencies:** Renderer strategy abstraction; configuration loader.

## 8. Snapshot Diffing

- **Status:** Not started
- **Goal:** Highlight structural changes between runs.
- **Scope:** Compare two exported trees (JSON) and annotate additions/removals in text/Markdown modes.
- **Rationale:** Valuable for PR reviews and release notes, natural extension of exporters.
- **Dependencies:** Stable JSON schema and difference engine utilities.

## 9. Cached Traversal & Incremental Updates

- **Status:** Not started
- **Goal:** Keep Arbor fast on massive repos.
- **Scope:** Cache directory snapshots keyed by mtime/hash, reuse results when unchanged, expose `--cache` controls.
- **Rationale:** Higher complexity but necessary for enterprise-scale trees, builds on prior features.
- **Dependencies:** Stable serialization, optional persistence directory, profiling hooks.

## 10. Performance Instrumentation

- **Status:** Not started
- **Goal:** Measure and tune Arbor
- **Scope:** Add timing summaries, node counts, and verbose logging switches, integrate with caching metrics.
- **Rationale:** Supports profiling and regression analysis before/after optimizations.
- **Dependencies:** Caching implementation; exporters for metrics.

## 11. Documentation & Pipeline Helpers

- **Status:** Not started
- **Goal:** Make Arbor output easy to embed.
- **Scope:** README snippet generator, plain text snapshot exporter, scripted integration samples.
- **Rationale:** Final polish once visualization/exporter work lands.
- **Dependencies:** Mature exporter suite, Markdown presets.

## 12. GitVersion & CI Matrix

- **Status:** Not started
- **Goal:** Automate releases and cross-platform validation.
- **Scope:** Configure GitVersion, add GitHub Actions for Windows/Linux, publish artifacts/tests.
- **Rationale:** Stabilizes release cadence after core features arrive, ensures parity across environments.
- **Dependencies:** Fast, reliable test suite.

## 13. Repo Polish & Governance

- **Status:** Not started
- **Goal:** Bring Arbor up to "professional project" expectations.
- **Scope:** Add LICENSE, CONTRIBUTING guide, issue/PR templates, `.editorconfig`, formatting/lint configuration,
  CODEOWNERS, Dependabot, and CI shields in the README.
- **Rationale:** Signals maturity, improves contributor experience, and automates basic maintenance tasks.
- **Dependencies:** Stabilized contributor workflow and documentation structure.
