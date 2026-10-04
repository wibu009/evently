import { useState } from "react"
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query"
import { toast } from "sonner"
import { Archive, Pencil, Plus } from "lucide-react"

import { categoriesApi } from "@/api/endpoints"
import { ErrorState, LoadingState, PageHeader } from "@/components/shared"
import { Badge } from "@/components/ui/badge"
import { Button } from "@/components/ui/button"
import { Card, CardContent } from "@/components/ui/card"
import {
  Dialog,
  DialogContent,
  DialogFooter,
  DialogHeader,
  DialogTitle
} from "@/components/ui/dialog"
import { Input } from "@/components/ui/input"
import { ApiError } from "@/lib/http"

export function CategoriesAdminPage() {
  const queryClient = useQueryClient()
  const [createOpen, setCreateOpen] = useState(false)
  const [editingId, setEditingId] = useState<string | null>(null)
  const [name, setName] = useState("")

  const categoriesQuery = useQuery({
    queryKey: ["categories"],
    queryFn: ({ signal }) => categoriesApi.list(signal)
  })

  const onError = (error: unknown) => toast.error(error instanceof ApiError ? error.detail : "Action failed")
  const refresh = () => void queryClient.invalidateQueries({ queryKey: ["categories"] })

  const createMutation = useMutation({
    mutationFn: () => categoriesApi.create(name.trim()),
    onSuccess: () => {
      toast.success("Category created")
      setCreateOpen(false)
      setName("")
      refresh()
    },
    onError
  })

  const updateMutation = useMutation({
    mutationFn: (args: { id: string; name: string }) => categoriesApi.update(args.id, args.name),
    onSuccess: () => {
      toast.success("Category renamed")
      setEditingId(null)
      refresh()
    },
    onError
  })

  const archiveMutation = useMutation({
    mutationFn: categoriesApi.archive,
    onSuccess: () => {
      toast.success("Category archived")
      refresh()
    },
    onError
  })

  return (
    <div>
      <PageHeader
        title="Categories"
        description="Organize the catalog. Archived categories disappear from creation forms."
        actions={
          <Button size="sm" onClick={() => setCreateOpen(true)}>
            <Plus className="size-4" /> New category
          </Button>
        }
      />

      {categoriesQuery.isPending ? (
        <LoadingState />
      ) : categoriesQuery.isError ? (
        <ErrorState message={String(categoriesQuery.error)} onRetry={() => categoriesQuery.refetch()} />
      ) : (
        <div className="space-y-2">
          {categoriesQuery.data.map((category) => (
            <Card key={category.id} className="rounded-2xl">
              <CardContent className="flex items-center justify-between p-3">
                {editingId === category.id ? (
                  <div className="flex flex-1 items-center gap-2">
                    <Input value={name} onChange={(e) => setName(e.target.value)} className="max-w-xs" />
                    <Button
                      size="sm"
                      onClick={() => updateMutation.mutate({ id: category.id, name: name.trim() })}
                      disabled={updateMutation.isPending || name.trim() === ""}
                    >
                      Save
                    </Button>
                    <Button size="sm" variant="ghost" onClick={() => setEditingId(null)}>
                      Cancel
                    </Button>
                  </div>
                ) : (
                  <>
                    <div className="flex items-center gap-3">
                      <p className="font-display font-semibold">{category.name}</p>
                      {category.isArchived && (
                        <Badge variant="outline" className="rounded-sm text-[10px]">
                          Archived
                        </Badge>
                      )}
                    </div>
                    {!category.isArchived && (
                      <div className="flex items-center gap-1">
                        <Button
                          size="icon"
                          variant="ghost"
                          aria-label="Rename"
                          onClick={() => {
                            setEditingId(category.id)
                            setName(category.name)
                          }}
                        >
                          <Pencil className="size-3.5" />
                        </Button>
                        <Button
                          size="icon"
                          variant="ghost"
                          aria-label="Archive"
                          disabled={archiveMutation.isPending}
                          onClick={() => archiveMutation.mutate(category.id)}
                        >
                          <Archive className="size-3.5" />
                        </Button>
                      </div>
                    )}
                  </>
                )}
              </CardContent>
            </Card>
          ))}
        </div>
      )}

      <Dialog open={createOpen} onOpenChange={setCreateOpen}>
        <DialogContent className="rounded-2xl">
          <DialogHeader>
            <DialogTitle className="font-display">New category</DialogTitle>
          </DialogHeader>
          <Input value={name} onChange={(e) => setName(e.target.value)} placeholder="e.g. Techno" />
          <DialogFooter>
            <Button variant="outline" onClick={() => setCreateOpen(false)}>
              Cancel
            </Button>
            <Button disabled={createMutation.isPending || name.trim() === ""} onClick={() => createMutation.mutate()}>
              Create
            </Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>
    </div>
  )
}

